using Intervu.Infrastructure.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace Intervu.API.Test.IntegrationTests
{
    public class PineconeIntegrationTests
    {
        private readonly IConfiguration _configuration;

        public PineconeIntegrationTests()
        {
            // Load configuration from appsettings.Development.json in the API project
            // Note: In a real CI environment, you'd use environment variables
            var projectDir = Directory.GetCurrentDirectory();
            // Try to find the API project root
            var apiProjectDir = Path.Combine(projectDir, "..", "..", "..", "..", "Intervu.API");
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectDir)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();
        }

        [Fact]
        public async Task Test_PineconeEmbedding_ShouldReturnValues()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var service = new PineconeInferenceService(httpClient, _configuration);

            // Act
            var result = await service.GetEmbeddingAsync("Hello, this is a test for coach search.");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            var configuredDimension = _configuration.GetValue<int?>("PineCone:PINECONE_EMBED_DIMENSION") ?? 1024;
            Assert.Equal(configuredDimension, result.Length);
        }

        [Fact]
        public async Task Test_PineconeVectorStore_UpsertAndSearch()
        {
            // Arrange
            var vectorStore = new PineconeVectorStoreService(_configuration);
            var embeddingService = new PineconeInferenceService(new HttpClient(), _configuration);
            
            var testId = "test-coach-id-" + System.Guid.NewGuid().ToString();
            var vector = await embeddingService.GetEmbeddingAsync("Specialist in React and .NET with 10 years experience");
            var metadata = new Dictionary<string, string>
            {
                { "name", "Test Coach" },
                { "bio", "Senior .NET Developer" }
            };

            // Act - Upsert
            await vectorStore.UpsertAsync(testId, vector, metadata);
            
            // Wait a bit for indexing (Pinecone is eventually consistent)
            await Task.Delay(2000);

            // Act - Search
            var searchResults = await vectorStore.SearchAsync(vector, 1);

            // Assert
            Assert.NotEmpty(searchResults);
            Assert.Contains(searchResults, r => r.Id == testId);

            // Cleanup
            await vectorStore.DeleteAsync(testId);
        }
    }
}
