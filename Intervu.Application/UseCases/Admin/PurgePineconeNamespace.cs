using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.Admin;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class PurgePineconeNamespace : IPurgePineconeNamespace
    {
        private readonly IVectorStoreService _vectorStoreService;

        public PurgePineconeNamespace(IVectorStoreService vectorStoreService)
        {
            _vectorStoreService = vectorStoreService;
        }

        public async Task ExecuteAsync(string @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Namespace cannot be empty.", nameof(@namespace));

            await _vectorStoreService.DeleteNamespaceAsync(@namespace);
        }
    }
}
