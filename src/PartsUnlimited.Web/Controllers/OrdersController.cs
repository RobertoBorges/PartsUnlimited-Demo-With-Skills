using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartsUnlimited.Utils;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrdersQuery _orders;
    private readonly IShippingTaxCalculator _calculator;

    public OrdersController(IOrdersQuery orders, IShippingTaxCalculator calculator)
    {
        _orders = orders;
        _calculator = calculator;
    }

    // GET /Orders?start=&end=
    public async Task<IActionResult> Index(DateTime? start, DateTime? end)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _orders.IndexHelperAsync(username, start, end, null);

        return View(new OrdersViewModel { Orders = orders });
    }

    // GET /Orders/Details/3
    public async Task<IActionResult> Details(int id)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orders.FindOrderAsync(id);

        if (order is null || order.Username != username)
            return NotFound();

        var costSummary = await _calculator.CalculateOrderCost(order);

        return View(new OrdersViewModel
        {
            SelectedOrder = order,
            OrderCostSummary = costSummary
        });
    }
}
