using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intervu.Application.DTOs;
using Intervu.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.Services
{
    public class GenerateAssessmentCatalogService : IGenerateAssessmentCatalogService
    {
        private const string ConfigSection = "AssessmentGenerationOptions";
        private readonly IConfiguration _configuration;

        public GenerateAssessmentCatalogService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<GenerateAssessmentOptionsResponse> GetOptionsAsync()
        {
            var section = _configuration.GetSection(ConfigSection);

            var response = new GenerateAssessmentOptionsResponse
            {
                TechStack = ReadValues(section, "TechStack"),
                Domain = ReadValues(section, "Domain")
            };

            return Task.FromResult(response);
        }

        private static List<string> ReadValues(IConfiguration section, string key)
        {
            return section.GetSection(key)
                .GetChildren()
                .Select(child => child.Value?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToList();
        }
    }
}
