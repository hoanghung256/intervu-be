using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Email;
using Intervu.Application.Interfaces.UseCases.Email;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;

namespace Intervu.Application.UseCases.Email
{
    public class SendBookingConfirmationEmail : ISendBookingConfirmationEmail
    {
        private readonly IEmailService _emailService;

        public SendBookingConfirmationEmail(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task ExecuteAsync(SendBookingConfirmationEmailDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrEmpty(dto.To))
                throw new ArgumentException("Email recipient is required", nameof(dto.To));

            // Build placeholders dictionary from DTO
            var placeholders = new Dictionary<string, string>
            {
                { "IntervieweeName", dto.CandidateName },
                { "BookingID", dto.BookingID },
                { "InterviewDate", dto.InterviewDate.ToString("MMMM dd, yyyy") },
                { "InterviewTime", dto.InterviewTime },
                { "Position", dto.Position },
                { "InterviewerName", dto.InterviewerName },
                { "Duration", dto.Duration.ToString() },
                { "JoinLink", dto.JoinLink },
                { "RescheduleLink", dto.RescheduleLink },
                { "FAQLink", "https://intervu.com/faq" },
                { "SupportLink", "https://intervu.com/support" },
                { "TermsLink", "https://intervu.com/terms" },
                { "PrivacyLink", "https://intervu.com/privacy" }
            };

            // Send email with template
            await _emailService.SendEmailWithTemplateAsync(
                dto.To,
                "BookingConfirmation",
                placeholders
            );
        }
    }
}