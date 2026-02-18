using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartsUnlimited.ProductSearch;
using PartsUnlimited.ViewModels;

namespace PartsUnlimited.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    private readonly IProductSearch _search;

    public SearchController(IProductSearch search) => _search = search;

    [HttpGet]
    public async Task<IActionResult> Index(string q)
    {
        var results = await _search.Search(q ?? string.Empty);
        return View(new SearchViewModel { Products = results, SearchQuery = q });
    }
}
