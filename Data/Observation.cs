using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Data;

/**
 * Observation of a cat, by a certain user at a certain time.  May include:
 * - text note
 * - one or more pictures of the cat (Photo entity)
 * - reference to a known cat (Cat entity)
 */
[Index(nameof(CatId))]
public class Observation
{
    public int Id { get; init; }
    public DateTime Date { get; set; }
    public string UserId { get; set; } = "";
    public IdentityUser User { get; set; } = null!;
    public string Description { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public Cat? Cat { get; set; }
    public int? CatId { get; set; }
    public IList<Photo> Photos { get; set; } = new List<Photo>();
}