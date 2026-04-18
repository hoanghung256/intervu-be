using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class PineconeIndexStatsDto
    {
        public int TotalVectorCount { get; set; }
        public int Dimension { get; set; }
        public Dictionary<string, int> Namespaces { get; set; } = new();
        public DateTime FetchedAt { get; set; }
    }
}
