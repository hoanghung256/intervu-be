using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAiConfiguration : IGetAiConfiguration
    {
        private readonly IConfiguration _configuration;

        public GetAiConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<List<AiConfigurationDto>> ExecuteAsync()
        {
            var configs = new List<AiConfigurationDto>
            {
                new AiConfigurationDto
                {
                    ServiceName = "Gemini API",
                    ModelName = _configuration["GeminiApi:ModelId"] ?? "(not set)",
                    Endpoint = "https://generativelanguage.googleapis.com",
                    HasApiKey = !string.IsNullOrWhiteSpace(_configuration["GeminiApi:GEMINI_API_KEY"]),
                    Purpose = "Reasoning / LLM Reranking"
                },
                new AiConfigurationDto
                {
                    ServiceName = "HuggingFace Router",
                    ModelName = _configuration["ReasoningApi:ModelId"] ?? "(not set)",
                    Endpoint = _configuration["ReasoningApi:BaseUrl"] ?? "(not set)",
                    HasApiKey = !string.IsNullOrWhiteSpace(_configuration["ReasoningApi:AI_API_KEY"]),
                    Purpose = "Reasoning / LLM Reranking (fallback)"
                },
                new AiConfigurationDto
                {
                    ServiceName = "Pinecone",
                    ModelName = _configuration["PineCone:PINECONE_EMBED_MODEL"] ?? "(not set)",
                    Endpoint = _configuration["PineCone:PINECONE_HOST_URL"] ?? "(not set)",
                    HasApiKey = !string.IsNullOrWhiteSpace(_configuration["PineCone:PINECONE_API_KEY"]),
                    Purpose = "Vector Store / Semantic Search"
                },
                new AiConfigurationDto
                {
                    ServiceName = "Python AI Service",
                    ModelName = "FastAPI (multi-model)",
                    Endpoint = _configuration["PythonAiService:BaseUrl"] ?? "(not set)",
                    HasApiKey = !string.IsNullOrWhiteSpace(_configuration["PythonAiService:BaseUrl"]),
                    Purpose = "CV Extraction / Assessment / Transcription"
                }
            };

            return Task.FromResult(configs);
        }
    }
}
