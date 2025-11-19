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
            // Strategy 1: Check for configured path (production deployment)
            var configPath = configuration["EmailSettings:TemplatesPath"];
            if (!string.IsNullOrEmpty(configPath) && Directory.Exists(configPath))
            {
                _templatesPath = configPath;
                return;
            }

            // Strategy 2: Look in base directory for EmailTemplates folder (production deployment - templates copied to output)
            var baseDirectory = AppContext.BaseDirectory;
            var localTemplatesPath = Path.Combine(baseDirectory, "EmailTemplates");
            if (Directory.Exists(localTemplatesPath))
            {
                _templatesPath = localTemplatesPath;
                return;
            }

            // Strategy 3: Navigate up from bin folder to find Infrastructure project (development)
            // Development structure: bin/Debug/net8.0 -> bin -> Debug -> Intervu.API -> SolutionRoot
            var currentDir = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 5; i++)
            {
                currentDir = currentDir.Parent;
                if (currentDir == null) break;

                var infrastructurePath = Path.Combine(
                    currentDir.FullName,
                    "Intervu.Infrastructure",
                    "ExternalServices",
                    "EmailServices",
                    "EmailTemplates"
                );

                if (Directory.Exists(infrastructurePath))
                {
                    _templatesPath = infrastructurePath;
                    return;
                }
            }

            // If all strategies fail, throw exception
            throw new DirectoryNotFoundException(
                $"Email templates directory not found. Searched locations:\n" +
                $"1. Config path: {configPath ?? "(not configured)"}\n" +
                $"2. Base directory: {localTemplatesPath}\n" +
                $"3. Infrastructure project path (from base: {baseDirectory})"
            );
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
