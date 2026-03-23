using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices.Pinecone
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text, string inputType = "passage");
        Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, string inputType = "passage");
    }
}
