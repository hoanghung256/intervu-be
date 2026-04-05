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
        private const string TemplatesFolderName = "EmailTemplates";

        public EmailTemplateService(IConfiguration configuration)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var searchedPaths = new List<string>();

            // Strategy 1: Check configured path (absolute or relative to base directory)
            var configPath = configuration["EmailSettings:TemplatesPath"];
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                var configCandidates = new List<string> { configPath };

                if (!Path.IsPathRooted(configPath))
                {
                    configCandidates.Add(Path.Combine(baseDirectory, configPath));
                }

                foreach (var configCandidate in configCandidates.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    searchedPaths.Add(configCandidate);
                    if (Directory.Exists(configCandidate))
                    {
                        _templatesPath = configCandidate;
                        return;
                    }
                }
            }

            // Strategy 2: Look in publish output directories
            var publishCandidates = new[]
            {
                Path.Combine(baseDirectory, "ExternalServices", "EmailServices", TemplatesFolderName),
                Path.Combine(baseDirectory, "ExternalServices", "EmailServices", "Templates"), // backward compatibility
                Path.Combine(baseDirectory, TemplatesFolderName)
            };

            foreach (var publishCandidate in publishCandidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                searchedPaths.Add(publishCandidate);
                if (Directory.Exists(publishCandidate))
                {
                    _templatesPath = publishCandidate;
                    return;
                }
            }

            // Strategy 3: Navigate up from bin folder to find source directory (development)
            var currentDir = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 8 && currentDir != null; i++)
            {
                var developmentCandidates = new[]
                {
                    Path.Combine(currentDir.FullName, "Intervu.Infrastructure", "ExternalServices", "EmailServices", TemplatesFolderName),
                    Path.Combine(currentDir.FullName, "ExternalServices", "EmailServices", TemplatesFolderName)
                };

                foreach (var developmentCandidate in developmentCandidates.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    searchedPaths.Add(developmentCandidate);
                    if (Directory.Exists(developmentCandidate))
                    {
                        _templatesPath = developmentCandidate;
                        return;
                    }
                }

                currentDir = currentDir.Parent;
            }

            var formattedSearchLocations = string.Join(
                Environment.NewLine,
                searchedPaths
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select((path, index) => $"{index + 1}. {path}"));

            throw new DirectoryNotFoundException(
                $"Email templates directory not found. Base directory: {baseDirectory}{Environment.NewLine}" +
                $"Searched locations:{Environment.NewLine}{formattedSearchLocations}"
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
