using System.ComponentModel.DataAnnotations;
using Kisse.Data;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Models;

/**
 * View model for cat observation.
 */
public record ObservationModel : IValidatableObject
{
	// Record id, read-only
	public int Id { get; init; }
	// User who created observation, read-only
	public string Username { get; init; } = "new";
	// Date of observation, read-only
	public DateTime Date { get; set; } = DateTime.Now;
	
	// Editable data from database record
	public IList<int> PhotoIds { get; set; } = new List<int>();
	public string Description { get; set; } = "";
	public double Lat { get; set; }
	public double Lng { get; set; }
	public int? CatId { get; set; }
	
	// Zoom setting for location view, not persisted to database but preserved through form submits
	public int Zoom { get; set; }
	
	// Photo records matching photoIds, for display only
	public IDictionary<int, Photo>? Photos { get; set; }
	// Cat record, for display only
	public CatModel? Cat { get; set; }

	public async Task ToEntity(Observation entity, ApplicationDbContext dbContext)
	{
		entity.Date = Date;
		entity.Description = Description ?? "";
		entity.Photos = new List<Photo>(await dbContext.Photos.Where(p => PhotoIds.Contains(p.Id)).ToListAsync());
		if (entity.Photos.Count > 0)
		{
			// if any photos are attached, date is forced to the date of the last photo
			// update it both in the entity and in the model
			entity.Date = Date = entity.Photos.Select(p => p.Date).Max();
		}
		entity.Lat = Lat;
		entity.Lng = Lng;
		entity.CatId = CatId;
	}

	public async Task<ObservationModel> LoadRelated(ApplicationDbContext dbContext)
	{
		Photos ??= await dbContext.Photos
			.Where(p => PhotoIds.Contains(p.Id))
			.ToDictionaryAsync(p => p.Id, p => p);

		if (CatId is not null && Cat is null)
		{
			Cat = CatModel.FromEntity((await dbContext.Cats.FindAsync(CatId))!);
		}

		return this;
	}

	public static ObservationModel FromEntity(Observation entity, bool skipParent = false)
	{
		return new ObservationModel
		{
			Id = entity.Id,
			Username = entity.User.UserName!,
			Date = entity.Date,
			Description = entity.Description,
			Lat = entity.Lat,
			Lng = entity.Lng,
			Photos = entity.Photos.ToDictionary(p => p.Id, p => p),
			PhotoIds = entity.Photos.Select(p => p.Id).ToList(),
			CatId = entity.CatId,
			Cat = entity.Cat is not null && !skipParent ? CatModel.FromEntity(entity.Cat) : null
		};
	}

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (String.IsNullOrWhiteSpace(Description) && PhotoIds.Count == 0)
		{
			yield return new ValidationResult("Observation must have either photos or comments",
				[nameof(Description)]);
		}
	}
}