using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.ExternalServices.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Intervu.Infrastructure.ExternalServices.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly string _appEmail;
        private readonly string _appPassword;
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailService(IEmailTemplateService emailTemplateService, IConfiguration configuration)
        {
            _emailTemplateService = emailTemplateService;
            _appEmail = configuration["EmailSettings:GmailEmail"];
            _appPassword = configuration["EmailSettings:GmailAppPassword"];
            
            if (string.IsNullOrEmpty(_appEmail) || string.IsNullOrEmpty(_appPassword))
                throw new InvalidOperationException("Email configuration is missing. Please check EmailSettings in appsettings.");
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(to))
                    throw new ArgumentException("Recipient email address cannot be empty.");
                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Email subject cannot be empty.");
                if (string.IsNullOrWhiteSpace(body))
                    throw new ArgumentException("Email body cannot be empty.");

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_appEmail, _appPassword);

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Intervu", _appEmail));
                    message.To.Add(new MailboxAddress("", to));
                    message.Subject = subject;

                    var contentType = isHtml ? "html" : "plain";
                    message.Body = new TextPart(contentType) { Text = body };

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Log error
                throw new InvalidOperationException($"Failed to send email to {to}", ex);
            }
        }

        public async Task SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, string> placeholders)
        {
            // Load the template content
            string templateContent = await _emailTemplateService.LoadTemplateAsync(templateName, placeholders);
            
            // Replace placeholders in the template
            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    if (!string.IsNullOrEmpty(placeholder.Value))
                        templateContent = templateContent.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }
            }
            
            // Generate a proper email subject based on template name
            string subject = GenerateSubject(templateName);
            
            // Send the email using the populated template
            await SendEmailAsync(to, subject, templateContent, isHtml: true);
        }
        
        private string GenerateSubject(string templateName)
        {
            return templateName switch
            {
                "BookingConfirmation" => "Your Interview Booking Confirmation",
                _ => "Intervu Notification"
            };
        }
    }
}
