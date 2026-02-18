using PartsUnlimited.Models;

namespace PartsUnlimited.Recommendations;

public interface IRecommendationEngine
{
    Task<List<Product>> GetRecommendationsAsync(int productId);
}
