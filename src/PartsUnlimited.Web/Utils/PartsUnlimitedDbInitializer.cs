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
        // Only seed if the DB is empty (no categories yet)
        if (await db.Categories.AnyAsync()) return;

        var brakes       = new Category { Name = "Brakes" };
        var lighting     = new Category { Name = "Lighting" };
        var wheels       = new Category { Name = "Wheels & Tires" };
        var batteries    = new Category { Name = "Batteries" };
        var oil          = new Category { Name = "Oil" };

        await db.Categories.AddRangeAsync(brakes, lighting, wheels, batteries, oil);
        await db.SaveChangesAsync();

        static string Details(params (string k, string v)[] pairs) =>
            JsonSerializer.Serialize(pairs.ToDictionary(p => p.k, p => p.v));

        var products = new List<Product>
        {
            // ── Lighting ──────────────────────────────────────────────────────
            new() { SkuNumber="LIG-0001", CategoryId=lighting.CategoryId, Title="Halogen Headlights (2-Pack)",
                    Price=38.99m, SalePrice=38.99m, Inventory=10, LeadTime=0, RecommendationId=1,
                    ProductArtUrl="product_lighting_headlight.jpg",
                    Description="Replace burned-out bulbs with DOT-approved halogen replacements.",
                    ProductDetails=Details(("Wattage","55"),("Type","Halogen"),("Fit","Universal")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="LIG-0002", CategoryId=lighting.CategoryId, Title="Bugeye Headlights (2-Pack)",
                    Price=48.99m, SalePrice=48.99m, Inventory=4, LeadTime=0, RecommendationId=2,
                    ProductArtUrl="product_lighting_bugeye-headlight.jpg",
                    Description="Distinctive bugeye-style headlights for a unique look.",
                    ProductDetails=Details(("Type","Bugeye"),("Fit","Universal")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="LIG-0003", CategoryId=lighting.CategoryId, Title="Turn Signal Light Bulb",
                    Price=6.99m, SalePrice=6.99m, Inventory=12, LeadTime=0, RecommendationId=3,
                    ProductArtUrl="product_lighting_lightbulb.jpg",
                    Description="Standard replacement turn-signal bulb fits most vehicles.",
                    ProductDetails=Details(("Wattage","21"),("Base","BAU15s")),
                    Created=DateTime.UtcNow },

            // ── Wheels & Tires ────────────────────────────────────────────────
            new() { SkuNumber="WHL-0001", CategoryId=wheels.CategoryId, Title="Matte Finish Rim",
                    Price=42.99m, SalePrice=42.99m, Inventory=4, LeadTime=0, RecommendationId=4,
                    ProductArtUrl="product_wheel_rim.jpg",
                    Description="Clean matte-finish steel rim suitable for most compact cars.",
                    ProductDetails=Details(("Diameter","15\""),("Width","6J"),("Finish","Matte")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="WHL-0002", CategoryId=wheels.CategoryId, Title="Blue Performance Alloy Rim",
                    Price=88.99m, SalePrice=88.99m, Inventory=4, LeadTime=0, RecommendationId=5,
                    ProductArtUrl="product_wheel_rim-blue.jpg",
                    Description="Lightweight alloy rim with a vivid blue custom finish.",
                    ProductDetails=Details(("Diameter","17\""),("Width","7J"),("Material","Alloy")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="WHL-0003", CategoryId=wheels.CategoryId, Title="High Performance Rim",
                    Price=88.99m, SalePrice=88.99m, Inventory=4, LeadTime=0, RecommendationId=6,
                    ProductArtUrl="product_wheel_rim-red.jpg",
                    Description="Sport-spec alloy rim for performance-oriented drivers.",
                    ProductDetails=Details(("Diameter","17\""),("Width","7.5J"),("Material","Alloy")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="WHL-0004", CategoryId=wheels.CategoryId, Title="Wheel Tire Combo",
                    Price=72.49m, SalePrice=72.49m, Inventory=0, LeadTime=7, RecommendationId=7,
                    ProductArtUrl="product_wheel_tyre-wheel-combo.jpg",
                    Description="Replacement all-season tire mounted on steel rim.",
                    ProductDetails=Details(("Size","185/65R15"),("Load Rating","88H")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="WHL-0005", CategoryId=wheels.CategoryId, Title="Chrome Rim Tire Combo",
                    Price=129.99m, SalePrice=129.99m, Inventory=4, LeadTime=0, RecommendationId=8,
                    ProductArtUrl="product_wheel_tyre-rim-chrome-combo.jpg",
                    Description="Chrome alloy rim with performance all-season tire included.",
                    ProductDetails=Details(("Size","195/55R16"),("Rim Finish","Chrome")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="WHL-0006", CategoryId=wheels.CategoryId, Title="Wheel Tire Combo (4 Pack)",
                    Price=219.99m, SalePrice=219.99m, Inventory=4, LeadTime=0, RecommendationId=9,
                    ProductArtUrl="product_wheel_tyre-wheel-combo-pack.jpg",
                    Description="Full set of four all-season tires mounted on steel rims.",
                    ProductDetails=Details(("Size","185/65R15"),("Quantity","4")),
                    Created=DateTime.UtcNow },

            // ── Brakes ────────────────────────────────────────────────────────
            new() { SkuNumber="BRK-0001", CategoryId=brakes.CategoryId, Title="Disk and Pad Combo",
                    Price=71.99m, SalePrice=71.99m, Inventory=4, LeadTime=0, RecommendationId=10,
                    ProductArtUrl="product_brakes_disk-pad-combo.jpg",
                    Description="Matched rotor and pad set for optimal braking performance.",
                    ProductDetails=Details(("Diameter","280 mm"),("Pad Material","Ceramic")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="BRK-0002", CategoryId=brakes.CategoryId, Title="Brake Rotor",
                    Price=25.99m, SalePrice=25.99m, Inventory=4, LeadTime=0, RecommendationId=11,
                    ProductArtUrl="product_brakes_disc.jpg",
                    Description="High-performance disc brake rotor for confident stopping power.",
                    ProductDetails=Details(("Diameter","280 mm"),("Material","Cast Iron")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="BRK-0003", CategoryId=brakes.CategoryId, Title="Brake Disk and Calipers",
                    Price=129.99m, SalePrice=129.99m, Inventory=4, LeadTime=0, RecommendationId=12,
                    ProductArtUrl="product_brakes_disc-calipers-red.jpg",
                    Description="Performance brake disc with red-painted calipers for a sporty finish.",
                    ProductDetails=Details(("Diameter","305 mm"),("Caliper Color","Red")),
                    Created=DateTime.UtcNow },

            // ── Batteries ─────────────────────────────────────────────────────
            new() { SkuNumber="BAT-0001", CategoryId=batteries.CategoryId, Title="12-Volt Calcium Battery",
                    Price=26.99m, SalePrice=26.99m, Inventory=8, LeadTime=0, RecommendationId=13,
                    ProductArtUrl="product_batteries_basic-battery.jpg",
                    Description="Reliable 12 V calcium-alloy battery for everyday vehicles.",
                    ProductDetails=Details(("Voltage","12V"),("CCA","430"),("Type","Calcium")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="BAT-0002", CategoryId=batteries.CategoryId, Title="Spiral Coil Battery",
                    Price=64.99m, SalePrice=64.99m, Inventory=4, LeadTime=0, RecommendationId=14,
                    ProductArtUrl="product_batteries_premium-battery.jpg",
                    Description="AGM spiral-coil battery with deep-cycle capability.",
                    ProductDetails=Details(("Voltage","12V"),("CCA","680"),("Type","AGM")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="BAT-0003", CategoryId=batteries.CategoryId, Title="Jumper Leads",
                    Price=16.99m, SalePrice=16.99m, Inventory=6, LeadTime=0, RecommendationId=15,
                    ProductArtUrl="product_batteries_jumper-leads.jpg",
                    Description="Heavy-duty jumper leads with insulated clamps, 4 m cable.",
                    ProductDetails=Details(("Length","4 m"),("Cable Gauge","10 AWG")),
                    Created=DateTime.UtcNow },

            // ── Oil ───────────────────────────────────────────────────────────
            new() { SkuNumber="OIL-0001", CategoryId=oil.CategoryId, Title="Filter Set",
                    Price=28.99m, SalePrice=28.99m, Inventory=4, LeadTime=0, RecommendationId=16,
                    ProductArtUrl="product_oil_filters.jpg",
                    Description="OEM-quality oil filter set for petrol and diesel engines.",
                    ProductDetails=Details(("Pack Size","5"),("Thread","M20x1.5")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="OIL-0002", CategoryId=oil.CategoryId, Title="Oil and Filter Combo",
                    Price=34.99m, SalePrice=34.99m, Inventory=4, LeadTime=0, RecommendationId=17,
                    ProductArtUrl="product_oil_oil-filter-combo.jpg",
                    Description="Full synthetic 5W-30 oil with matching filter for one complete service.",
                    ProductDetails=Details(("Volume","5 L"),("Viscosity","5W-30"),("Standard","ACEA A3/B4")),
                    Created=DateTime.UtcNow },

            new() { SkuNumber="OIL-0003", CategoryId=oil.CategoryId, Title="Synthetic Engine Oil",
                    Price=22.99m, SalePrice=22.99m, Inventory=8, LeadTime=0, RecommendationId=18,
                    ProductArtUrl="product_oil_premium-oil.jpg",
                    Description="Premium full-synthetic engine oil for extended drain intervals.",
                    ProductDetails=Details(("Volume","4 L"),("Viscosity","0W-40"),("Standard","VW 502/505")),
                    Created=DateTime.UtcNow },
        };

        await db.Products.AddRangeAsync(products);

        if (!await db.Stores.AnyAsync())
        {
            await db.Stores.AddRangeAsync(
                new Store { Name = "Spokane" },
                new Store { Name = "Bellevue" },
                new Store { Name = "Tacoma" });
        }

        await db.SaveChangesAsync();
    }
}
