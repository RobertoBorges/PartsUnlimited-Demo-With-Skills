using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

/// <summary>
/// Seeds the database with sample data.
/// Called from Program.cs during development startup after EF migrations.
/// </summary>
public static class PartsUnlimitedDbInitializer
{
    public static async Task SeedAsync(PartsUnlimitedContext db)
    {
        if (await db.Categories.AnyAsync()) return; // already seeded

        var categories = new List<Category>
        {
            new() { Name = "Brakes" },
            new() { Name = "Lighting" },
            new() { Name = "Wheels & Tires" },
            new() { Name = "Batteries" },
            new() { Name = "Oil" }
        };

        await db.Categories.AddRangeAsync(categories);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            new()
            {
                SkuNumber = "LIG-0001",
                Title = "Halogen Headlights (2-Pack)",
                Price = 38.99m,
                SalePrice = 38.99m,
                ProductArtUrl = "product_lights_headlight_halogen.jpg",
                CategoryId = categories.Single(c => c.Name == "Lighting").CategoryId,
                Description = "Replace burned out bulbs with these DOT-approved halogen replacement bulbs.",
                ProductDetails = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Wattage", "55" },
                    { "Type", "Halogen" },
                    { "Fit", "Universal" }
                }),
                Inventory = 10,
                LeadTime = 0,
                RecommendationId = 1,
                Created = DateTime.UtcNow
            },
            new()
            {
                SkuNumber = "BRA-0001",
                Title = "Brake Rotor",
                Price = 25.99m,
                SalePrice = 25.99m,
                ProductArtUrl = "product_brakes_disc.jpg",
                CategoryId = categories.Single(c => c.Name == "Brakes").CategoryId,
                Description = "A high-performance disc brake component.",
                ProductDetails = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Diameter", "280 mm" },
                    { "Material", "Cast Iron" }
                }),
                Inventory = 4,
                LeadTime = 0,
                RecommendationId = 2,
                Created = DateTime.UtcNow
            },
            new()
            {
                SkuNumber = "WHE-0001",
                Title = "Wheel Tire Combo",
                Price = 72.49m,
                SalePrice = 72.49m,
                ProductArtUrl = "product_wheel_rimtire.jpg",
                CategoryId = categories.Single(c => c.Name == "Wheels & Tires").CategoryId,
                Description = "Replacement all-season tire mounted on steel rim.",
                ProductDetails = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Size", "185/65R15" },
                    { "Load Rating", "88H" }
                }),
                Inventory = 0,
                LeadTime = 7,
                RecommendationId = 3,
                Created = DateTime.UtcNow
            }
        };

        await db.Products.AddRangeAsync(products);

        var stores = new List<Store>
        {
            new() { Name = "Spokane" },
            new() { Name = "Bellevue" },
            new() { Name = "Tacoma" }
        };

        await db.Stores.AddRangeAsync(stores);
        await db.SaveChangesAsync();
    }
}
