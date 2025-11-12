using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using slick.Application.Services.Interfaces;
using System.Net.Mime;

namespace slick.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SMTPSettings _smtpSettings;
        private static readonly Dictionary<string, string> _templateCache = new();

        public EmailService(IOptions<SMTPSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
        }

        public async Task SendPasswordResetEmail(string email, string firstName, string newPassword)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "PasswordResetEmail.html");
            var htmlBody = await LoadTemplateAsync(templatePath);
            htmlBody = htmlBody
                .Replace("{firstName}", firstName)
                .Replace("{password}", newPassword)
                .Replace("{Year}", DateTime.UtcNow.Year.ToString())
                .Replace("{SupportUrl}", "https://yourapp.com/support");

            var subject = "New Password for Finance Mangement System";
            await SendEmailAsync(email, subject, htmlBody, includeLogo: true);
        }

        public async Task SendVerificationEmail(string email, string verificationToken, string userName)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            //var htmlBody = await LoadTemplateAsync("VerificationEmail.html");
            // var htmlBody = await LoadTemplateAsync(Path.Combine("Templates", "VerificationEmail.html"));
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "VerificationEmail.html");
            var htmlBody = await LoadTemplateAsync(templatePath);
            var verifyUrl = $"{verificationToken}";
            htmlBody = htmlBody
                .Replace("{UserName}", userName)
                .Replace("{VerifyUrl}", verificationToken)
                .Replace("{Year}", DateTime.UtcNow.Year.ToString())
                .Replace("{SupportUrl}", "https://yourapp.com/support");

            var subject = "Email Verification";
            await SendEmailAsync(email, subject, htmlBody, includeLogo: true);
        }

        public async Task SendEmailNotificationAsync(IEnumerable<string> emails, string subject, string message, string userName = "User")
        {
            if (emails == null)
                throw new ArgumentNullException(nameof(emails));

            var emailList = emails.Where(email => !string.IsNullOrWhiteSpace(email)).Distinct().ToList();
            if (!emailList.Any())
                throw new ArgumentException("At least one valid email address must be provided.", nameof(emails));

            var htmlBody = await LoadTemplateAsync("NotificationEmail.html");
            htmlBody = htmlBody
                .Replace("{UserName}", userName)
                .Replace("{Subject}", subject)
                .Replace("{Message}", message)
                .Replace("{Year}", DateTime.UtcNow.Year.ToString())
                .Replace("{SupportUrl}", "https://yourapp.com/support");

            var tasks = emailList.Select(email => SendEmailAsync(email, subject, htmlBody, includeLogo: true));
            await Task.WhenAll(tasks);
        }

        public async Task SendEmailNotificationAsync(string email, string subject, string message, string userName = "User")
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            var htmlBody = await LoadTemplateAsync("NotificationEmail.html");
            htmlBody = htmlBody
                .Replace("{UserName}", userName)
                .Replace("{Subject}", subject)
                .Replace("{Message}", message)
                .Replace("{Year}", DateTime.UtcNow.Year.ToString())
                .Replace("{SupportUrl}", "https://yourapp.com/support");

            await SendEmailAsync(email, subject, htmlBody, includeLogo: true);
        }

        private async Task<string> LoadTemplateAsync(string templateName)
        {
            if (_templateCache.TryGetValue(templateName, out var template))
            {
                return template;
            }

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateName);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templateName}", templatePath);
            }

            template = await File.ReadAllTextAsync(templatePath);
            _templateCache[templateName] = template;
            return template;
        }

        private async Task SendEmailAsync(string email, string subject, string body, bool includeLogo)
        {
            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.SmtpUsername, "Marcon Business Group Inc."),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(new MailAddress(email));

                if (includeLogo)
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "marcon.png");
                    if (!File.Exists(logoPath))
                    {
                        throw new FileNotFoundException("Logo image not found.", logoPath);
                    }

                    var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    var logoAttachment = new LinkedResource(logoPath, "image/png")
                    {
                        ContentId = "logo",
                        TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                        ContentType = new ContentType(MediaTypeNames.Image.Png)
                        {
                            Name = "HabeshaGebeya.png" // 👈 This is important
                        },
                    };
                    htmlView.LinkedResources.Add(logoAttachment);
                    mailMessage.AlternateViews.Add(htmlView);
                }
                else
                {
                    // Fallback for plain HTML without embedded resources
                    mailMessage.Body = body;
                }

                using var smtpClient = new SmtpClient(_smtpSettings.SmtpServer)
                {
                    Port = _smtpSettings.SmtpPort,
                    Credentials = new NetworkCredential(_smtpSettings.SmtpUsername, _smtpSettings.SmtpPassword),
                    EnableSsl = true // Ensure SSL is enabled for security
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException($"Failed to send email to {email}: SMTP error.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while sending email to {email}.", ex);
            }
        }
    }
}