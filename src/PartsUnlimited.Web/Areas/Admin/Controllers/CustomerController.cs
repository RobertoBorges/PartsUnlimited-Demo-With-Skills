using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Areas.Admin.Controllers;

public class CustomerController : AdminController
{
    private readonly IPartsUnlimitedContext _db;

    public CustomerController(IPartsUnlimitedContext db) => _db = db;

    // GET /Admin/Customer
    // Filter by Entra ID username (OID or UPN stored in Order.Username)
    public async Task<IActionResult> Index(
        string? username, string? email, string? phone)
    {
        var query = _db.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(o => o.Username != null && o.Username.Contains(username));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(o => o.Email.Contains(email));

        if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(o => o.Phone.Contains(phone));

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Take(100)
            .ToListAsync();

        return View(orders);
    }
}
