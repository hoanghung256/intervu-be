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
            // Get base directory of the running application (e.g., bin/Debug/net8.0)
            var baseDirectory = AppContext.BaseDirectory;
            
            // Navigate up from bin/Debug/net8.0 to solution root
            // bin/Debug/net8.0 -> bin -> Debug -> Intervu.API -> SolutionRoot
            var solutionRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            
            if (solutionRoot == null)
                throw new InvalidOperationException("Could not determine solution root directory.");
            
            // Build path to EmailTemplates folder in Infrastructure project
            // Structure: SolutionRoot/Intervu.Infrastructure/ExternalServices/EmailServices/EmailTemplates
            _templatesPath = Path.Combine(
                solutionRoot, 
                "Intervu.Infrastructure", 
                "ExternalServices", 
                "EmailServices", 
                "EmailTemplates"
            );
            
            // Validate that the templates directory exists
            if (!Directory.Exists(_templatesPath))
                throw new DirectoryNotFoundException($"Email templates directory not found at: {_templatesPath}");
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
