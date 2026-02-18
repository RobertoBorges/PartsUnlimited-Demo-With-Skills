using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PartsUnlimited.Models;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IPartsUnlimitedContext _db;
    private readonly IMemoryCache _cache;

    public HomeController(IPartsUnlimitedContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var topSelling = await _cache.GetOrCreateAsync("topselling", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.OrderDetails.Count)
                .Take(4)
                .ToListAsync();
        });

        var newArrivals = await _cache.GetOrCreateAsync("newarrivals", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Created)
                .Take(4)
                .ToListAsync();
        });

        var categories = await _db.Categories.ToListAsync();

        var vm = new HomeViewModel
        {
            TopSellingProducts = topSelling ?? [],
            NewProducts = newArrivals ?? [],
            Categories = categories
        };

        return View(vm);
    }

    public IActionResult Error() => View();
}
