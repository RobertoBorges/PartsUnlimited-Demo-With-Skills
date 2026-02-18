using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IPartsUnlimitedContext _db;

    public ProductsController(IPartsUnlimitedContext db) => _db = db;

    // GET /api/products?sale=true
    [HttpGet]
    public async Task<ActionResult<List<Product>>> Get([FromQuery] bool sale = false)
    {
        var query = _db.Products.AsQueryable();
        if (sale) query = query.Where(p => p.Price != p.SalePrice);
        return await query.ToListAsync();
    }

    // GET /api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();
        return product;
    }
}
