using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PartsUnlimited.Hubs;
using PartsUnlimited.Models;

namespace PartsUnlimited.Areas.Admin.Controllers;

public class StoreManagerController : AdminController
{
    private readonly IPartsUnlimitedContext _db;
    private readonly IMemoryCache _cache;
    private readonly IHubContext<AnnouncementHub> _hub;

    public StoreManagerController(
        IPartsUnlimitedContext db,
        IMemoryCache cache,
        IHubContext<AnnouncementHub> hub)
    {
        _db = db;
        _cache = cache;
        _hub = hub;
    }

    // GET /Admin/StoreManager
    public async Task<IActionResult> Index(
        string sortField = "Name", string sortDirection = "Up")
    {
        var products = await _db.Products.Include(p => p.Category).ToListAsync();

        products = (sortField, sortDirection) switch
        {
            ("Name", "Up")    => products.OrderBy(p => p.Title).ToList(),
            ("Name", "Down")  => products.OrderByDescending(p => p.Title).ToList(),
            ("Price", "Up")   => products.OrderBy(p => p.Price).ToList(),
            ("Price", "Down") => products.OrderByDescending(p => p.Price).ToList(),
            _ => products.OrderBy(p => p.Title).ToList()
        };

        return View(products);
    }

    // GET /Admin/StoreManager/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _cache.GetOrCreateAsync($"product_{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        });

        if (product is null) return NotFound();
        return View(product);
    }

    // GET /Admin/StoreManager/Create
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Product { Created = DateTime.UtcNow });
    }

    // POST /Admin/StoreManager/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(product);
        }

        product.Created = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Notify connected clients via SignalR (server → client push)
        await _hub.Clients.All.SendAsync("announcement", new
        {
            product.Title,
            Url = Url.Action("Details", "Store",
                new { id = product.ProductId, area = string.Empty })
        });

        _cache.Remove("latestProduct");
        return RedirectToAction(nameof(Index));
    }

    // GET /Admin/StoreManager/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        await PopulateCategoriesAsync(product.CategoryId);
        return View(product);
    }

    // POST /Admin/StoreManager/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        _db.Entry(product).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        _cache.Remove($"product_{product.ProductId}");
        return RedirectToAction(nameof(Index));
    }

    // GET /Admin/StoreManager/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();
        return View(product);
    }

    // POST /Admin/StoreManager/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is not null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            _cache.Remove($"product_{id}");
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        ViewBag.CategoryId = new SelectList(
            await _db.Categories.ToListAsync(), "CategoryId", "Name", selectedId);
    }
}
