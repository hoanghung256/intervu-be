using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Intervu.Application.DTOs;
using Intervu.Application.Interfaces.ExternalServices;
using Newtonsoft.Json;

namespace Intervu.Application.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AiServiceClient");
        }

        public async Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request)
        {
            if (request == null)
            {
                return new GenerateAssessmentResponse();
            }

            var response = await _httpClient.PostAsJsonAsync("api/generate-assessment", request);

            var rawContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"AI service request failed with status code {(int)response.StatusCode}: {rawContent}");
            }

            GenerateAssessmentResponse? result = null;

            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<GenerateAssessmentResponse>(rawContent, options);
                }
                catch (System.Text.Json.JsonException)
                {
                    result = null;
                }
            }

            if (result == null)
            {
                result = new GenerateAssessmentResponse();
            }

            result.PhaseA ??= new System.Collections.Generic.List<AssessmentQuestionItemDto>();
            result.PhaseB ??= new System.Collections.Generic.List<AssessmentQuestionItemDto>();

            foreach (var item in result.PhaseA)
            {
                item.Options ??= new System.Collections.Generic.List<OptionDto>();
            }

            foreach (var item in result.PhaseB)
            {
                item.Options ??= new System.Collections.Generic.List<OptionDto>();
            }

            return result;
        }
    }
}
