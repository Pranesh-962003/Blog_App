using System.Text;
using System.Text.Json;

namespace Backend.Services
{
    public class EmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public EmbeddingService(HttpClient http, IConfiguration config)
        {
            _http = new HttpClient();
            _apiKey = config["HuggingFace:ApiKey"]!;
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<float[]> GetEmbedding(string input, bool isQuery = false)
        {
            string prefix = isQuery ? "query: " : "passage: ";
            var payload = new { inputs = new[] { prefix + input } };
            var json = JsonSerializer.Serialize(payload);

            var response = await _http.PostAsync(
                "https://api-inference.huggingface.co/models/intfloat/multilingual-e5-large-instruct",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();
            var vectors = JsonSerializer.Deserialize<List<List<float>>>(result);
            return vectors?[0]?.ToArray() ?? Array.Empty<float>();
        }
    }
}
