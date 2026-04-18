using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class AiEndpointSeriesPointDto
    {
        public DateTime Bucket { get; set; }
        public Dictionary<string, int> CountByEndpoint { get; set; } = new();
    }
}
