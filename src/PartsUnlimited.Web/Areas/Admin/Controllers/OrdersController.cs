using Microsoft.AspNetCore.Mvc;
using PartsUnlimited.Utils;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Areas.Admin.Controllers;

public class OrdersController : AdminController
{
    private readonly IOrdersQuery _orders;
    private readonly IShippingTaxCalculator _calculator;

    public OrdersController(IOrdersQuery orders, IShippingTaxCalculator calculator)
    {
        _orders = orders;
        _calculator = calculator;
    }

    // GET /Admin/Orders
    public async Task<IActionResult> Index(
        string? username, DateTime? start, DateTime? end, string? search)
    {
        var orders = await _orders.IndexHelperAsync(username, start, end, search);
        return View(new OrdersViewModel { Orders = orders });
    }

    // GET /Admin/Orders/Details/3
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.FindOrderAsync(id);
        if (order is null) return NotFound();

        var costSummary = await _calculator.CalculateOrderCost(order);

        return View(new OrdersViewModel
        {
            SelectedOrder = order,
            OrderCostSummary = costSummary
        });
    }
}
