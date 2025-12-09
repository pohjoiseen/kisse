using System.Diagnostics;
using Kisse.Data;
using Microsoft.AspNetCore.Mvc;
using Kisse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Controllers;

public class HomeController(ApplicationDbContext dbContext) : Controller
{
    /// <summary>
    /// Shows the main map page of the app, with cats and unlinked observation markers. 
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new IndexMapModel
        {
            Cats = await dbContext.Cats
                .ToDictionaryAsync(c => c.Id, c => (c.Lat, c.Lng)),
            Observations = await dbContext.Observations
                .Where(o => o.CatId == null)
                .ToDictionaryAsync(o => o.Id, o => (o.Lat, o.Lng))
        };
            
        return View(model);
    }

    /// <summary>
    /// Default error page.
    /// </summary>
    /// <returns></returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Blank page, useful for Htmx in some cases.  Make sure to cache for a long time.
    /// </summary>
    /// <returns></returns>
    [ResponseCache(Duration = 7 * 86400)]
    [HttpGet]
    public IActionResult Blank()
    {
        return Ok("");
    }
}
