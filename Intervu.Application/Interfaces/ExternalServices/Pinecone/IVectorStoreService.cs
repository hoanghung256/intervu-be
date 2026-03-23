using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices.Pinecone
{
    public interface IVectorStoreService
    {
        Task UpsertAsync(string id, float[] vector, Dictionary<string, string> metadata);
        Task<List<VectorMatch>> SearchAsync(float[] queryVector, int topK = 5);
        Task DeleteAsync(string id);
    }

    public record VectorMatch(string Id, double Score, Dictionary<string, string>? Metadata);
}
