using PartsUnlimited.Models;

namespace PartsUnlimited.ViewModels;

public class SearchViewModel
{
    public List<Product> Products { get; set; } = [];
    public string? SearchQuery { get; set; }
}
