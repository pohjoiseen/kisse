using Htmx;
using Kisse.Data;
using Kisse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Controllers;

[Authorize]
public class CatController(ApplicationDbContext dbContext) : Controller
{
    private static readonly int PageSize = 25;
        
    /// <summary>
    /// Shows a page with a paginated list of all cats.
    /// </summary>
    /// <param name="page">Page number, 1-indexed</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var cats = await dbContext.CatsWithRelated
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Description)
            .Skip((page - 1) *  PageSize).Take(PageSize)
            .ToListAsync();
        var catModels = await Task.WhenAll(cats.Select(c => CatModel.FromEntity(c).LoadRelated(dbContext)));
        var totalCats = dbContext.Cats.Count();
        return View(new ListModel<CatModel>
        {
            Data = catModels,
            Page = page,
            TotalData = totalCats,
            TotalPages = totalCats / PageSize + (totalCats % PageSize > 0 ? 1 : 0)
        });
    }
    
    /// <summary>
    /// Shows a popup page which allows picking a cat (to link it with observation) or to add a new one.
    /// </summary>
    /// <param name="lat">Latitude (to look for nearest cats)</param>
    /// <param name="lng">Longitude (to look for nearest cats)</param>
    /// <param name="id">Currently selected cat id, if any</param>
    /// <param name="cancel">Cancel button pressed</param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Pick(double lat, double lng, int? id, bool cancel)
    {
        if (cancel)
        {
            return Ok("");
        }
        
        var model = new PickCatModel
        {
            Id = id,
            Cat = id is not null 
                ? await CatModel.FromEntity((await dbContext.CatsWithRelated.FirstOrDefaultAsync(c => c.Id == id))!)
                    .LoadRelated(dbContext)
                : null,
            Lat = lat,
            Lng = lng,
            NearestCats = await Task.WhenAll((await dbContext.CatsWithRelated
                .OrderBy(c => Math.Abs(c.Lat - lat) + Math.Abs(c.Lng - lng))
                .Take(10)
                .ToListAsync())
                .Select(c => CatModel.FromEntity(c).LoadRelated(dbContext))
                .ToList())
        };
        return View(model);
    }
    
    /// <summary>
    /// Shows a popup page which allows adding a new cat.  This is displayed from Pick()
    /// and is currently the only way to add a cat (thus, through adding/editing an observation).
    /// </summary>
    /// <param name="cat">Cat being added</param>
    /// <param name="cancel">Cancel button pressed</param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> AddFromPick(CatModel cat, bool cancel)
    {
        if (cancel)
        {
            return Ok("");
        }

        if (HttpContext.Request.Method == "GET")
        {
            ModelState.Clear();
        }
        
        if (HttpContext.Request.Method == "POST" && ModelState.IsValid)
        {
            var entity = new Cat();
            cat.ToEntity(entity);
            dbContext.Cats.Add(entity);
            await dbContext.SaveChangesAsync();
            cat = CatModel.FromEntity(entity);
        }
        
        return View(cat);
    }
    
    /// <summary>
    /// Shows a page which allows editing an existing cat.
    /// </summary>
    /// <param name="id">Cat id</param>
    /// <param name="catModel">Cat being edited</param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, CatModel catModel)
    {
        var entity = await dbContext.Cats.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }
        
        if (HttpContext.Request.Method == "GET")
        {
            catModel = CatModel.FromEntity(entity);
            ModelState.Clear();
        }
        
        await catModel.LoadRelated(dbContext);
        
        if (HttpContext.Request.Method == "POST" && ModelState.IsValid)
        {
            catModel.ToEntity(entity);
            await dbContext.SaveChangesAsync();
        }
        
        return View(catModel);
    }

    /// <summary>
    /// Shows a small summary snippet about a cat.  Meant to display as map popups.
    /// </summary>
    /// <param name="id">Cat id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ViewPopup(int id)
    {
        var entity = await dbContext.CatsWithRelated.FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        var cat = CatModel.FromEntity(entity);
        await cat.LoadRelated(dbContext);

        return View(cat);
    }
    
    /// <summary>
    /// Deletes a cat, unlinks its observations from it (but does not delete them).
    /// </summary>
    /// <param name="id">Cat id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await dbContext.CatsWithRelated
            .FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        foreach (var observation in entity.Observations)
        {
            observation.CatId = null;
        }
        dbContext.Cats.Remove(entity);
        await dbContext.SaveChangesAsync();

        Response.Htmx(headers => headers.Redirect(Url.Action("Index", "Home")!));
        return NoContent();
    }
}