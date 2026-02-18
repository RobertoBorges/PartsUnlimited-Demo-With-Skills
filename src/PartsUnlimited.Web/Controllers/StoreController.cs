using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PartsUnlimited.Models;
using PartsUnlimited.Recommendations;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Controllers;

[AllowAnonymous]
public class StoreController : Controller
{
    private readonly IPartsUnlimitedContext _db;
    private readonly IMemoryCache _cache;
    private readonly IRecommendationEngine _recommendations;
    private readonly IConfiguration _config;

    public StoreController(
        IPartsUnlimitedContext db,
        IMemoryCache cache,
        IRecommendationEngine recommendations,
        IConfiguration config)
    {
        _db = db;
        _cache = cache;
        _recommendations = recommendations;
        _config = config;
    }

    // GET /Store
    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories.ToListAsync();
        return View(categories);
    }

    // GET /Store/Browse?categoryId=3
    public async Task<IActionResult> Browse(int categoryId)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category is null) return NotFound();

        var vm = new ProductsViewModel
        {
            Products = category.Products,
            Category = category.Name
        };

        return View(vm);
    }

    // GET /Store/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _cache.GetOrCreateAsync($"product_{id}", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await _db.Products
                .Include(p => p.Category)
                .SingleOrDefaultAsync(p => p.ProductId == id);
        });

        if (product is null) return NotFound();

        var showRecommendations = _config.GetValue<bool>("AppSettings:ShowRecommendations");
        var recs = showRecommendations
            ? await _recommendations.GetRecommendationsAsync(id)
            : [];

        var vm = new ProductViewModel
        {
            Product = product,
            Recommendations = recs
        };

        return View(vm);
    }
}
