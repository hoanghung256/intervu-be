using Microsoft.Playwright;
using System.Text.Json;

namespace Intervu.API.Test.Utils
{
    public static class TestUtils
    {
        public static StringContent CreateJsonContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        public static T? DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        
        public static async Task<T?> GetAndDeserialize<T>(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return DeserializeJson<T>(content);
        }
    }
}
