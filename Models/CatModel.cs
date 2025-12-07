using System.ComponentModel.DataAnnotations;
using Kisse.Data;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Models;

public record CatModel
{
    // Record id, read-only
    public int? Id { get; init; }

    // Editable fields
    [Required] [MaxLength(255)] public string Name { get; set; } = "";
    public string? Description { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    
    // Photo records from latest observations, for display only
    public IList<Photo>? LatestPhotos { get; set; }
    public static readonly int LatestPhotosNum = 20;
    
    // Observations, for display only
    public IList<ObservationModel>? Observations { get; set; }

    public void ToEntity(Cat entity)
    {
        entity.Name = Name;
        entity.Description = Description ?? "";
        entity.Lat = Lat;
        entity.Lng = Lng;
    }

    public async Task<CatModel> LoadRelated(ApplicationDbContext dbContext)
    {
        if (Id is null)
        {
            return this;
        }

        var entity = await dbContext.CatsWithRelated.FirstAsync(c => c.Id == Id);
        LatestPhotos ??= entity.GetLatestPhotos(LatestPhotosNum).ToList();
        Observations ??= entity.Observations.Select(o => ObservationModel.FromEntity(o, true)).ToList();
        return this;
    }

    public static CatModel FromEntity(Cat entity)
    {
        return new CatModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Lat = entity.Lat,
            Lng = entity.Lng
        };
    }
}