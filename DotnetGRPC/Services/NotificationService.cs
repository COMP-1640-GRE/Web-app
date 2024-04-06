using System.Net.Mail;
using DotnetGRPC.Model.DTO;
using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Grpc.Core;
using System.Net;

namespace DotnetGRPC.Services
{

    public class NotificationService : Notification.NotificationBase
    {
        private readonly AppDbContext _dbContext;
        private readonly EmailSettings _emailSettings;
        private readonly UserRepository _userRepository;
        private readonly TemplateRepository _templateRepository;
        private readonly NotificationRepository _notificationRepository;


        public NotificationService(AppDbContext dbContext, IOptions<EmailSettings> emailSettings, UserRepository userRepository, TemplateRepository templateRepository, NotificationRepository notificationRepository)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings.Value;
            _userRepository = userRepository;
            _templateRepository = templateRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task SendEmailAsync(long studentUserId, string templateCode, long staffUserId, string option)
        {
            if (studentUserId == 0)
            {
                throw new ArgumentException("Student is null");
            }

            var student = await _userRepository.FindByIdAsync(studentUserId);
            var staff = await _userRepository.FindByIdAsync(staffUserId);
            var template = await _templateRepository.FindByTemplateCodeAsync(templateCode);
            using (var mailMessage = new MailMessage())
            {
                mailMessage.To.Add(new MailAddress(student.Email));
                mailMessage.From = new MailAddress(staff.Email);
                mailMessage.Subject = template.TemplateName;
                mailMessage.Body = template.TemplateContent
                    .Replace("[Student Name]", $"{student.FirstName} {student.LastName}")
                    .Replace("[Name]", $"{staff.FirstName} {staff.LastName}")
                    .Replace("[Role]", staff.Role.ToString())
                    .Replace("[choose appropriate option]", option);
                mailMessage.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
                {
                    smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    smtpClient.EnableSsl = _emailSettings.EnableSsl;
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }

        public override async Task<NotificationResponse> sendNotification(NotificationRequest request, ServerCallContext context)
        {
            if (request.WithEmail)
            {
                await SendEmailAsync(request.StudentUserid, request.TemplateCode.Replace("noti", "email"), request.StaffUserid, request.Option);
            }

            var template = await _templateRepository.FindByTemplateCodeAsync(request.TemplateCode);
            var content = template.TemplateContent.Replace("[choose appropriate option]", request.Option);

            var notification = new Model.Notification
            {
                Content = content,
                Seen = false,
                UserId = request.StudentUserid,
                WithEmail = request.WithEmail
            };

            await _notificationRepository.SaveAsync(notification);

            var notificationResponse = new NotificationResponse
            {
                Content = content,
                StudentUserid = notification.UserId,
            };

            return notificationResponse;
        }

        public override async Task getListNotifications(ListNotificationRequest request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
        {
            var notifications = await _notificationRepository.GetNotifications(request.StudentUserid, request.Seen);

            foreach (var notification in notifications)
            {
                var notificationResponse = new NotificationResponse
                {
                    Content = notification.Content,
                    StudentUserid = notification.UserId,
                };

                await responseStream.WriteAsync(notificationResponse);
            }
        }
    }
}
