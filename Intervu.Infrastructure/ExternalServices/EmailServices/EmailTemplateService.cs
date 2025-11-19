using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Infrastructure.ExternalServices.EmailServices;
using Microsoft.Extensions.Configuration;

namespace Intervu.Infrastructure.ExternalServices.EmailServices
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatesPath;

        public EmailTemplateService(IConfiguration configuration)
        {
            // Get the path from config, or use default
            var configPath = configuration["EmailSettings:TemplatesPath"];
            
            if (Path.IsPathRooted(configPath))
            {
                // If it's an absolute path, use it directly
                _templatesPath = configPath;
            }
            else
            {
                // Otherwise, combine with base directory
                _templatesPath = Path.Combine(AppContext.BaseDirectory, configPath ?? "EmailTemplates");
            }
        }
        public async Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> placeholders)
        {
            var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template not found: {templatePath}");
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);

            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    if (!string.IsNullOrEmpty(placeholder.Value))
                    {
                        var key = $"{{{{{placeholder.Key}}}}}";
                        templateContent = templateContent.Replace(key, placeholder.Value);
                    }
                }
            }
            return templateContent;
        }
    }
}
