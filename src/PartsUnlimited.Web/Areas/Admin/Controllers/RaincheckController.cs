using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Areas.Admin.Controllers;

public class RaincheckController : AdminController
{
    private readonly IPartsUnlimitedContext _db;

    public RaincheckController(IPartsUnlimitedContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var rainchecks = await _db.RainChecks
            .Include(r => r.Store)
            .Include(r => r.Product)
            .ToListAsync();
        return View(rainchecks);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Raincheck raincheck)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(raincheck);
        }

        _db.RainChecks.Add(raincheck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await _db.RainChecks
            .Include(rc => rc.Store)
            .Include(rc => rc.Product)
            .SingleOrDefaultAsync(rc => rc.RaincheckId == id);
        if (r is null) return NotFound();
        return View(r);
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.StoreId = new SelectList(await _db.Stores.ToListAsync(), "StoreId", "Name");
        ViewBag.ProductId = new SelectList(await _db.Products.ToListAsync(), "ProductId", "Title");
    }
}
