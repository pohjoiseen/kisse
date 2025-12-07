using System.Diagnostics;
using Kisse.Data;
using Microsoft.AspNetCore.Mvc;
using Kisse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Controllers;

public class HomeController(ApplicationDbContext dbContext) : Controller
{
    [Authorize]
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [ResponseCache(Duration = 7 * 86400)]
    public IActionResult Blank()
    {
        return Ok("");
    }
}
