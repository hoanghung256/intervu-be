using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Admin;

namespace Intervu.Application.Interfaces.ExternalServices.Pinecone
{
    public interface IVectorStoreService
    {
        Task UpsertAsync(string id, float[] vector, Dictionary<string, string> metadata, string? @namespace = null);
        Task<List<VectorMatch>> SearchAsync(
            float[] queryVector,
            int topK = 5,
            string? @namespace = null,
            Dictionary<string, string>? metadataFilter = null);
        Task DeleteAsync(string id, string? @namespace = null);
        Task<PineconeIndexStatsDto> DescribeIndexStatsAsync();
    }

    public record VectorMatch(string Id, double Score, Dictionary<string, string>? Metadata);
}
