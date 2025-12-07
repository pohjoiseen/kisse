using System.Security.Claims;
using Htmx;
using Kisse.Data;
using Kisse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Controllers;

[Authorize]
public class ObservationController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext,
                                   IConfiguration configuration) : Controller
{
    private static readonly int PageSize = 25;
        
    public async Task<IActionResult> Index(int page = 1)
    {
        var observations = await dbContext.ObservationsWithRelated
            .OrderByDescending(o => o.Date)
            .Skip((page - 1) *  PageSize).Take(PageSize)
            .ToListAsync();
        var observationModels = await Task.WhenAll(observations.Select(o => ObservationModel.FromEntity(o).LoadRelated(dbContext)));
        var totalObservations = dbContext.Observations.Count();
        return View(new ListModel<ObservationModel>
        {
            Data = observationModels,
            Page = page,
            TotalData = totalObservations,
            TotalPages = totalObservations / PageSize + (totalObservations % PageSize > 0 ? 1 : 0)
        });
    }

    public async Task<IActionResult> Add(ObservationModel observationModel)
    {
        if (HttpContext.Request.Method == "GET")
        {
            observationModel = new ObservationModel
            {
                Username = User.FindFirstValue(ClaimTypes.Name)!,
                Lat = configuration.GetValue<double>("DefaultLocation:Lat"),
                Lng = configuration.GetValue<double>("DefaultLocation:Lng"),
                Zoom = configuration.GetValue<int>("DefaultLocation:MaxZoom"),
            };
            ModelState.Clear();
        }
        
        await observationModel.LoadRelated(dbContext);

        if (HttpContext.Request.Method == "POST" && ModelState.IsValid)
        {
            var entity = new Observation
            {
                User = (await userManager.GetUserAsync(User))!,
            };
            await observationModel.ToEntity(entity, dbContext);
            dbContext.Observations.Add(entity);
            await dbContext.SaveChangesAsync();
            Response.Htmx(headers => headers.Redirect(Url.Action("Edit", new { id = entity.Id })!));
        }
        
        return View(observationModel);
    }
    
    public async Task<IActionResult> Edit(int id, ObservationModel observationModel)
    {
        var entity = await dbContext.ObservationsWithRelated
            .FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        if (HttpContext.Request.Method == "GET")
        {
            observationModel = ObservationModel.FromEntity(entity);
            observationModel.Zoom = configuration.GetValue<int>("DefaultLocation:MaxZoom");
            ModelState.Clear();
        }
        
        await observationModel.LoadRelated(dbContext);
        
        if (HttpContext.Request.Method == "POST" && ModelState.IsValid)
        {
            if (entity.User.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }
            
            await observationModel.ToEntity(entity, dbContext);
            await dbContext.SaveChangesAsync();
        }
        
        return View(observationModel);
    }

    public async Task<IActionResult> ViewPopup(int id)
    {
        var entity = await dbContext.ObservationsWithRelated
            .FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        var observation = ObservationModel.FromEntity(entity);

        return View(observation);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await dbContext.ObservationsWithRelated
            .FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null)
        {
            return NotFound();
        }
        if (entity.User.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        foreach (var photo in entity.Photos)
        {
            try
            {
                System.IO.File.Delete(photo.ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
                System.IO.File.Delete(photo.OriginalFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
                Directory.Delete(Path.GetDirectoryName(photo.ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]))!);
            }
            catch
            {
                // ignore possible errors
            }

            dbContext.Photos.Remove(photo);
        }
        dbContext.Observations.Remove(entity);
        await dbContext.SaveChangesAsync();

        Response.Htmx(headers => headers.Redirect(Url.Action("Index", "Home")!));
        return NoContent();
    }
}