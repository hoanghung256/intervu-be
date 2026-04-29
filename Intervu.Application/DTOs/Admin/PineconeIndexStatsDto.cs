using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class PineconeIndexStatsDto
    {
        public int TotalVectorCount { get; set; }
        public int Dimension { get; set; }
        public Dictionary<string, NamespaceComparisonDto> Namespaces { get; set; } = new();
        public DateTime FetchedAt { get; set; }
    }

    public class NamespaceComparisonDto
    {
        public int VectorCount { get; set; }
        public int SqlCount { get; set; }
        public int Delta => VectorCount - SqlCount;
        public string Status => Delta == 0 ? "Synced" : Delta > 0 ? "Excess" : "Stale";
    }
}
