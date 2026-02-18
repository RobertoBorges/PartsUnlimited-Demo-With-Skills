using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

public interface IRaincheckQuery
{
    Task<List<Raincheck>> GetAllRainchecksAsync();
    Task<Raincheck?> FindRaincheckAsync(int id);
}

public class RaincheckQuery : IRaincheckQuery
{
    private readonly IPartsUnlimitedContext _db;

    public RaincheckQuery(IPartsUnlimitedContext db) => _db = db;

    public async Task<List<Raincheck>> GetAllRainchecksAsync()
        => await _db.RainChecks
            .Include(r => r.Store)
            .Include(r => r.Product)
            .ToListAsync();

    public async Task<Raincheck?> FindRaincheckAsync(int id)
        => await _db.RainChecks
            .Include(r => r.Store)
            .Include(r => r.Product)
            .SingleOrDefaultAsync(r => r.RaincheckId == id);
}
