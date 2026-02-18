using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Api;

[ApiController]
[Route("api/[controller]")]
public class RaincheckApiController : ControllerBase
{
    private readonly IPartsUnlimitedContext _db;

    public RaincheckApiController(IPartsUnlimitedContext db) => _db = db;

    // GET /api/raincheck
    [HttpGet]
    public async Task<ActionResult<List<Raincheck>>> Get()
        => await _db.RainChecks
            .Include(r => r.Store)
            .Include(r => r.Product)
            .ToListAsync();

    // GET /api/raincheck/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Raincheck>> Get(int id)
    {
        var r = await _db.RainChecks
            .Include(rc => rc.Store)
            .Include(rc => rc.Product)
            .SingleOrDefaultAsync(rc => rc.RaincheckId == id);
        return r is null ? NotFound() : r;
    }

    // POST /api/raincheck
    [HttpPost]
    public async Task<ActionResult<Raincheck>> Post(Raincheck raincheck)
    {
        _db.RainChecks.Add(raincheck);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = raincheck.RaincheckId }, raincheck);
    }
}
