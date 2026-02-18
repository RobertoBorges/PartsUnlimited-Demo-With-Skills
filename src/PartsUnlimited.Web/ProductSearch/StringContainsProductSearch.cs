using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.ProductSearch;

/// <summary>
/// Simple in-database substring search. Migrated from EF6 to EF Core 8.
/// </summary>
public class StringContainsProductSearch : IProductSearch
{
    private readonly IPartsUnlimitedContext _db;

    public StringContainsProductSearch(IPartsUnlimitedContext db) => _db = db;

    public async Task<List<Product>> Search(string query)
        => await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
            .ToListAsync();
}
