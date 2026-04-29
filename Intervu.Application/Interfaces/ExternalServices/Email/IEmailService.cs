using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, string> placeholders);

        /// <summary>
        /// Paginates over all users with the given role and sends an email to each.
        /// Designed to be called from a Hangfire background job to avoid memory bloat.
        /// </summary>
        Task BroadcastEmailToRoleAsync(string role, string templateName, Dictionary<string, string> sharedPlaceholders);
    }
}
