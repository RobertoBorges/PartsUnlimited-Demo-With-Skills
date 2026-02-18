using PartsUnlimited.Models;

namespace PartsUnlimited.ViewModels;

public class ShoppingCartViewModel
{
    public List<CartItem> CartItems { get; set; } = [];
    public decimal CartTotal { get; set; }
}
