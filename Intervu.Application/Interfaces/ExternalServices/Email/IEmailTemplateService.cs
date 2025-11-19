using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices.Email
{
    public interface IEmailTemplateService
    {
        Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> placeholders);
    }
}
