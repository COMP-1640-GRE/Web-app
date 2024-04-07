using System.Net.Mail;
using DotnetGRPC.Model.DTO;
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

        public async Task SendEmailAsync(long studentUserId, string templateCode, string option)
        {
            if (studentUserId == 0)
            {
                throw new ArgumentException("Student is null");
            }

            var student = await _userRepository.FindByIdAsync(studentUserId) ?? throw new ArgumentException("Student not found");
            var template = await _templateRepository.FindByTemplateCodeAsync(templateCode) ?? throw new ArgumentException("Template not found");
            using (var mailMessage = new MailMessage())
            {
                mailMessage.To.Add(new MailAddress(student.Email));
                mailMessage.From = new MailAddress("hungcuong28597@gmail.com");
                mailMessage.Subject = template.TemplateName;
                mailMessage.Body = template.TemplateContent
                    .Replace("[Student Name]", $"{student.FirstName} {student.LastName}")
                    .Replace("[choose appropriate option]", option)
                    .Replace("[6-Letter Code]", option);
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
            var content = string.Empty;

            if (request.TemplateCode != "reset_pw_email")
            {
                if (request.WithEmail)
                {
                    await SendEmailAsync(request.UserId, request.TemplateCode.Replace("noti", "email"), request.Option);
                }
                var template = await _templateRepository.FindByTemplateCodeAsync(request.TemplateCode);
                content = template.TemplateContent.Replace("[choose appropriate option]", request.Option);

                var notification = new Model.Notification
                {
                    Content = content,
                    Seen = false,
                    UserId = request.UserId,
                    WithEmail = request.WithEmail
                };

                await _notificationRepository.SaveAsync(notification);
            }
            else
            {
                await SendEmailAsync(request.UserId, request.TemplateCode, request.Option);
            }
            var notificationResponse = new NotificationResponse
            {
                Content = content,
                UserId = request.UserId,
                WithEmail = request.WithEmail
            };

            return notificationResponse;
        }

        public override async Task getListNotifications(ListNotificationRequest request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
        {
            var notifications = await _notificationRepository.GetNotifications(request.UserId, request.Seen);

            foreach (var notification in notifications)
            {
                var notificationResponse = new NotificationResponse
                {
                    Content = notification.Content,
                    UserId = notification.UserId,
                };

                await responseStream.WriteAsync(notificationResponse);
            }
        }
    }
}

