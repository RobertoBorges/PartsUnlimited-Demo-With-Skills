using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

public interface IOrdersQuery
{
    Task<List<Order>> IndexHelperAsync(string? username, DateTime? start, DateTime? end, string? search);
    Task<Order?> FindOrderAsync(int orderId);
}

public class OrdersQuery : IOrdersQuery
{
    private readonly IPartsUnlimitedContext _db;

    public OrdersQuery(IPartsUnlimitedContext db) => _db = db;

    public async Task<List<Order>> IndexHelperAsync(
        string? username, DateTime? start, DateTime? end, string? search)
    {
        var query = _db.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(o => o.Username == username);

        if (start.HasValue)
            query = query.Where(o => o.OrderDate >= start.Value);

        if (end.HasValue)
            query = query.Where(o => o.OrderDate <= end.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o =>
                o.Name.Contains(search) ||
                o.Email.Contains(search) ||
                o.Address.Contains(search));

        return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
    }

    public async Task<Order?> FindOrderAsync(int orderId)
        => await _db.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .SingleOrDefaultAsync(o => o.OrderId == orderId);
}
