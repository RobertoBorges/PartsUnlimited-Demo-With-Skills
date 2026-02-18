using PartsUnlimited.Models;

namespace PartsUnlimited.ViewModels;

public class ProductsViewModel
{
    public List<Product> Products { get; set; } = [];
    public required string Category { get; set; }
}
