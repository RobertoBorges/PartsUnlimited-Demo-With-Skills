using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartsUnlimited.Models;

namespace PartsUnlimited.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly IPartsUnlimitedContext _db;
    private const string PromoCode = "FREE";

    public CheckoutController(IPartsUnlimitedContext db) => _db = db;

    // GET /Checkout/AddressAndPayment
    public IActionResult AddressAndPayment()
    {
        // Pre-fill name and email from Entra ID claims
        var order = new Order
        {
            Name = User.Identity?.Name ?? string.Empty,
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Username = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        return View(order);
    }

    // POST /Checkout/AddressAndPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddressAndPayment([Bind(
        "Name,Address,City,State,PostalCode,Country,Phone,Email")] Order order)
    {
        if (!ModelState.IsValid) return View(order);

        var formPromo = HttpContext.Request.Form["PromoCode"].ToString();
        if (!string.Equals(formPromo, PromoCode, StringComparison.OrdinalIgnoreCase))
        {
            return View(order);
        }

        order.Username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        order.OrderDate = DateTime.UtcNow;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var cart = Models.ShoppingCart.GetCart(_db, HttpContext);
        await cart.CreateOrder(order);

        return RedirectToAction("Complete", new { id = order.OrderId });
    }

    // GET /Checkout/Complete/3
    public async Task<IActionResult> Complete(int id)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _db.Orders.FindAsync(id);

        if (order is null || order.Username != username)
            return View("Error");

        return View(order);
    }
}
