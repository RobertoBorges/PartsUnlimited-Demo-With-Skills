using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Recommendations;

/// <summary>
/// Fallback recommendation engine that returns products with the same recommendation group ID.
/// Migrated from EF6 to EF Core 8; logic unchanged.
/// </summary>
public class NaiveRecommendationEngine : IRecommendationEngine
{
    private readonly IPartsUnlimitedContext _db;

    public NaiveRecommendationEngine(IPartsUnlimitedContext db) => _db = db;

    public async Task<List<Product>> GetRecommendationsAsync(int productId)
    {
        var product = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == productId);
        if (product is null) return [];

        return await _db.Products
            .Where(p => p.RecommendationId == product.RecommendationId && p.ProductId != productId)
            .Take(3)
            .ToListAsync();
    }
}
