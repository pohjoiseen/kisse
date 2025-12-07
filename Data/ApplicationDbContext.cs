using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kisse.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Cat> Cats { get; init; }
    public DbSet<Observation> Observations { get; init; }
    public DbSet<Photo> Photos { get; init; }

    public IQueryable<Cat> CatsWithRelated =>
        Cats
            .Include(c => c.Observations)
            .ThenInclude(o => o.User)
            .Include(c => c.Observations)
            .ThenInclude(o => o.Photos);

    public IQueryable<Observation> ObservationsWithRelated =>
        Observations
            .Include(o => o.Cat)
            .Include(o => o.User)
            .Include(o => o.Photos);
}
