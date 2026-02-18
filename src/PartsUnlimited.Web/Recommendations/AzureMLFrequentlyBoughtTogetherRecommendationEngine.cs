using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PartsUnlimited.Models;

namespace PartsUnlimited.Recommendations;

/// <summary>
/// Azure ML-backed recommendation engine.
/// Falls back to empty list on failure rather than throwing.
/// Migrated: HttpClient injected via IHttpClientFactory (DI), not new HttpClient().
/// </summary>
public class AzureMLFrequentlyBoughtTogetherRecommendationEngine : IRecommendationEngine
{
    private readonly IPartsUnlimitedContext _db;
    private readonly HttpClient _httpClient;
    private readonly string? _mlEndpoint;
    private readonly string? _mlApiKey;

    public AzureMLFrequentlyBoughtTogetherRecommendationEngine(
        IPartsUnlimitedContext db,
        HttpClient httpClient,
        IConfiguration config)
    {
        _db = db;
        _httpClient = httpClient;
        _mlEndpoint = config["MachineLearning:Url"];
        _mlApiKey = config["MachineLearning:Key"];
    }

    public async Task<List<Product>> GetRecommendationsAsync(int productId)
    {
        if (string.IsNullOrWhiteSpace(_mlEndpoint))
            return [];

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _mlEndpoint);
            if (!string.IsNullOrWhiteSpace(_mlApiKey))
                request.Headers.Add("Authorization", $"Bearer {_mlApiKey}");

            var payload = JsonSerializer.Serialize(new { productId });
            request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<List<int>>(body);

            if (ids is null || ids.Count == 0) return [];

            return await _db.Products
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();
        }
        catch
        {
            return [];
        }
    }
}
