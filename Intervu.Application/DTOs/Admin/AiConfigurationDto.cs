namespace Intervu.Application.DTOs.Admin
{
    public class AiConfigurationDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public bool HasApiKey { get; set; }
        public string Purpose { get; set; } = string.Empty;
    }
}
