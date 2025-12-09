using System.ComponentModel.DataAnnotations;

namespace Kisse.Data;

/// <summary>
/// Known cat.  May have any number of observations linked to it, "name" and text note.
/// Same cat may be modified by multiple users.
/// </summary>
public class Cat
{
    public int Id { get; init; }
    [MaxLength(255)]
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    // Lat and Lng are updated automatically by database triggers (as average of all associated observations),
    // see CatCoordsUpdateTriggers migration
    public double Lat { get; set; }
    public double Lng { get; set; }
    public IList<Observation> Observations { get; set; } = new List<Observation>();

    public IEnumerable<Photo> GetLatestPhotos(int count) =>
        Observations.OrderByDescending(o => o.Date).SelectMany(o => o.Photos, (o, p) => p).Take(count);
}
