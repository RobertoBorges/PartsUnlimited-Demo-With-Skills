using PartsUnlimited.Models;

namespace PartsUnlimited.ViewModels;

public class OrdersViewModel
{
    public List<Order> Orders { get; set; } = [];
    public Order? SelectedOrder { get; set; }
    public OrderCostSummary? OrderCostSummary { get; set; }
}
