using PartsUnlimited.Models;

namespace PartsUnlimited.ProductSearch;

public interface IProductSearch
{
    Task<List<Product>> Search(string query);
}
