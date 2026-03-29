using System;
using System.Threading.Tasks;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.SmartSearch;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SmartSearchExtractDataFromFile : ISmartSearchExtractDataFromFile
    {
        private readonly IPythonAiService _pythonAiService;

        public SmartSearchExtractDataFromFile(IPythonAiService pythonAiService)
        {
            _pythonAiService = pythonAiService;
        }

        public async Task<string> ExecuteAsync(SmartSearchExtractRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("File is required and cannot be empty.");
            }

            using var stream = request.File.OpenReadStream();

            // This will throw if the Python service fails, which will be caught by the Controller's 500 handler
            var jsonResponse = await _pythonAiService.ExtractDocumentToJsonAsync(
                stream,
                request.File.FileName,
                request.DocType);

            return jsonResponse;
        }
    }
}
