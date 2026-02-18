using PartsUnlimited.Models;

namespace PartsUnlimited.ViewModels;

public class ProductViewModel
{
    public required Product Product { get; set; }
    public List<Product> Recommendations { get; set; } = [];
}
