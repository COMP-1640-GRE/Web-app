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
        private readonly ContributionRepository _contributionRepository;
        private readonly FacultyRepository _facultyRepository;


        public NotificationService(AppDbContext dbContext, IOptions<EmailSettings> emailSettings, UserRepository userRepository, TemplateRepository templateRepository, NotificationRepository notificationRepository, ContributionRepository contributionRepository, FacultyRepository facultyRepository)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings.Value;
            _userRepository = userRepository;
            _templateRepository = templateRepository;
            _notificationRepository = notificationRepository;
            _contributionRepository = contributionRepository;
            _facultyRepository = facultyRepository;
        }

        public async Task SendEmailAsync(long userId, string templateCode, string option)
        {
            if (userId == 0)
            {
                throw new ArgumentException("Student is null");
            }

            var student = await _userRepository.FindByIdAsync(userId) ?? throw new ArgumentException("Student not found");
            var template = await _templateRepository.FindByTemplateCodeAsync(templateCode) ?? throw new ArgumentException("Template not found");
            using (var mailMessage = new MailMessage())
            {
                mailMessage.To.Add(new MailAddress(student.Email));
                mailMessage.From = new MailAddress(_emailSettings.Username);
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

        public async Task SendNotifyPendingContribution()
        {
            Console.WriteLine("Sending notification to faculty coordinators for pending contributions");
            var contributions = await _contributionRepository.FindPendingContributions14DaysAgo();
            if (contributions.Count == 0)
            {
                Console.WriteLine("No pending contributions");
            }
            foreach (var contribution in contributions)
            {
                var templateNotification = await _templateRepository.FindByTemplateCodeAsync("fac01_noti");
                var templateEmail = await _templateRepository.FindByTemplateCodeAsync("fac01_email");
                var student = await _userRepository.FindByIdAsync(contribution.DbAuthorId);
                if (student.FacultyId == null)
                {
                    throw new Exception("Student has no faculty");
                }
                var faculty = await _facultyRepository.FindByIdAsync(student.FacultyId.Value);
                var users = await _userRepository.FindFacultyCoordinators(student.FacultyId.Value);
                foreach (var user in users)
                {
                    var notification = new Model.Notification
                    {
                        Content = templateNotification.TemplateContent,
                        Seen = false,
                        UserId = user.Id,
                        WithEmail = true
                    };
                    await _notificationRepository.SaveAsync(notification);

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.To.Add(new MailAddress(user.Email));
                        mailMessage.From = new MailAddress(_emailSettings.Username);
                        mailMessage.Subject = templateEmail.TemplateName;
                        mailMessage.Body = templateEmail.TemplateContent
                            .Replace("[Student Name]", $"{student.FirstName} {student.LastName}")
                            .Replace("[Title of Contribution]", contribution.Title)
                            .Replace("[Date of Submission]", contribution.UpdatedAt.ToString("dd/MM/yyyy"))
                            .Replace("[Faculty Name]", faculty.Name);
                        mailMessage.IsBodyHtml = true;
                        using (var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
                        {
                            smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                            smtpClient.EnableSsl = _emailSettings.EnableSsl;
                            await smtpClient.SendMailAsync(mailMessage);
                        }
                    }
                    Console.WriteLine("Email sent to " + faculty.Name + " faculty coordinator");
                }
            }
        }
    }
}

