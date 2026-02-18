using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;
using PartsUnlimited.Utils;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Controllers;

public class ShoppingCartController : Controller
{
    private readonly IPartsUnlimitedContext _db;
    private readonly IShippingTaxCalculator _calculator;
    private readonly ITelemetryProvider _telemetry;

    public ShoppingCartController(
        IPartsUnlimitedContext db,
        IShippingTaxCalculator calculator,
        ITelemetryProvider telemetry)
    {
        _db = db;
        _calculator = calculator;
        _telemetry = telemetry;
    }

    // GET /ShoppingCart
    public async Task<IActionResult> Index()
    {
        var cart = ShoppingCart.GetCart(_db, HttpContext);
        var items = await cart.GetCartItems();
        var count = items.Sum(i => i.Count);

        var fakeOrder = new Order { OrderDetails = items.Select(i => new OrderDetail
        {
            Count = i.Count,
            UnitPrice = i.UnitPrice,
            ProductId = i.ProductId
        }).ToList() };

        var costSummary = await _calculator.CalculateOrderCost(fakeOrder);

        var vm = new ShoppingCartViewModel
        {
            CartItems = items,
            CartTotal = await cart.GetTotal()
        };

        ViewBag.OrderCostSummary = costSummary;
        _telemetry.TrackTrace("Cart/Server/Index");
        return View(vm);
    }

    // GET /ShoppingCart/AddToCart/5
    [HttpGet]
    public async Task<IActionResult> AddToCart(int id)
    {
        var product = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();

        var cart = ShoppingCart.GetCart(_db, HttpContext);
        await cart.AddToCart(product);

        return RedirectToAction("Index");
    }

    // POST /ShoppingCart/RemoveFromCart
    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var cart = ShoppingCart.GetCart(_db, HttpContext);
        var itemCount = await cart.RemoveFromCart(id);
        var cartTotal = await cart.GetTotal();
        var cartCount = await cart.GetCount();

        var vm = new ShoppingCartRemoveViewModel
        {
            Message = "1 item has been removed from your shopping cart.",
            CartTotal = cartTotal,
            CartCount = cartCount,
            ItemCount = itemCount,
            DeleteId = id
        };

        return Json(vm);
    }

    // GET /ShoppingCart/CartSummary (used as partial via ViewComponent pattern)
    public IActionResult CartSummary() => PartialView();
}
