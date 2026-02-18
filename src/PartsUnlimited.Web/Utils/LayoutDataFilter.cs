using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

/// <summary>
/// Global action filter that populates ViewBag data needed by _Layout.cshtml
/// (categories for nav, cart summary, latest product for announcement banner).
/// </summary>
public class LayoutDataFilter : IAsyncActionFilter
{
    private readonly IPartsUnlimitedContext _db;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LayoutDataFilter(IPartsUnlimitedContext db, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Populate categories for the nav category bar
        var categories = await _cache.GetOrCreateAsync("nav_categories", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Categories.ToListAsync();
        });

        // Populate latest product for announcement banner
        var latestProduct = await _cache.GetOrCreateAsync("latestProduct", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Products.OrderByDescending(p => p.Created).FirstOrDefaultAsync();
        });

        if (context.Controller is Microsoft.AspNetCore.Mvc.Controller controller)
        {
            controller.ViewBag.Categories = categories ?? new List<Category>();
            controller.ViewBag.Product = latestProduct;
        }

        await next();
    }
}
