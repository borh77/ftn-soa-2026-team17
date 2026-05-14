using Microsoft.EntityFrameworkCore;

namespace TouristApp.Tours.Infrastructure.Database;

/// <summary>
/// EF Core DbContext for the Tours module.
/// Add DbSets and entity configurations
/// </summary>
public class ToursContext : DbContext
{
    public ToursContext(DbContextOptions<ToursContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tours"); // might change in the future, but for now this is fine
        base.OnModelCreating(modelBuilder);
    }
}
