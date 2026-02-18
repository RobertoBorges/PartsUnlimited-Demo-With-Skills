using Microsoft.EntityFrameworkCore;

namespace PartsUnlimited.Models;

/// <summary>
/// Shopping cart backed by a session cookie (GUID).
/// Refactored from MVC5 HttpContextBase to ASP.NET Core HttpContext / IHttpContextAccessor.
/// </summary>
public class ShoppingCart
{
    private readonly IPartsUnlimitedContext _db;
    private readonly string _cartId;

    private const string SessionKey = "Session";

    private ShoppingCart(IPartsUnlimitedContext db, string cartId)
    {
        _db = db;
        _cartId = cartId;
    }

    // ── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves (or creates) the cart for the current request.
    /// The cart ID is persisted in a "Session" cookie as a GUID string.
    /// </summary>
    public static ShoppingCart GetCart(IPartsUnlimitedContext db, HttpContext context)
    {
        var cartId = GetCartId(context);
        return new ShoppingCart(db, cartId);
    }

    private static string GetCartId(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(SessionKey, out var cartId)
            || string.IsNullOrEmpty(cartId))
        {
            cartId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append(SessionKey, cartId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
        }

        return cartId;
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task AddToCart(Product product)
    {
        var cartItem = await _db.CartItems
            .SingleOrDefaultAsync(c => c.CartId == _cartId && c.ProductId == product.ProductId);

        if (cartItem is null)
        {
            cartItem = new CartItem
            {
                CartId = _cartId,
                ProductId = product.ProductId,
                Count = 1,
                DateCreated = DateTime.UtcNow,
                UnitPrice = product.Price
            };
            await _db.CartItems.AddAsync(cartItem);
        }
        else
        {
            cartItem.Count++;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<int> RemoveFromCart(int id)
    {
        var cartItem = await _db.CartItems
            .SingleOrDefaultAsync(c => c.CartId == _cartId && c.CartItemId == id);

        if (cartItem is null) return 0;

        if (cartItem.Count > 1)
        {
            cartItem.Count--;
        }
        else
        {
            _db.CartItems.Remove(cartItem);
        }

        await _db.SaveChangesAsync();
        return cartItem.Count;
    }

    public async Task EmptyCart()
    {
        var items = _db.CartItems.Where(c => c.CartId == _cartId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartItems()
        => await _db.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.CartId == _cartId)
                    .ToListAsync();

    public async Task<int> GetCount()
        => await _db.CartItems
                    .Where(c => c.CartId == _cartId)
                    .SumAsync(c => (int?)c.Count) ?? 0;

    public async Task<decimal> GetTotal()
        => await _db.CartItems
                    .Where(c => c.CartId == _cartId)
                    .SumAsync(c => (decimal?)c.Count * c.UnitPrice) ?? 0m;

    // ── Check-out ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts the cart into an Order, snapshotting unit prices at time of purchase.
    /// </summary>
    public async Task<int> CreateOrder(Order order)
    {
        var cartTotal = 0m;
        var cartItems = await GetCartItems();

        foreach (var item in cartItems)
        {
            var product = await _db.Products.SingleAsync(p => p.ProductId == item.ProductId);
            var unitPrice = product.Price;

            var orderDetail = new OrderDetail
            {
                ProductId = item.ProductId,
                OrderId = order.OrderId,
                UnitPrice = unitPrice,
                Count = item.Count
            };

            cartTotal += item.Count * unitPrice;
            order.OrderDetails.Add(orderDetail);
        }

        order.Total = cartTotal;
        await EmptyCart();
        return order.OrderId;
    }
}
