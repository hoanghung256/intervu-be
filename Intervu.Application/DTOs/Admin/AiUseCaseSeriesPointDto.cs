using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class AiUseCaseSeriesPointDto
    {
        public DateTime Bucket { get; set; }
        public Dictionary<string, int> CountByUseCase { get; set; } = new();
    }
}
