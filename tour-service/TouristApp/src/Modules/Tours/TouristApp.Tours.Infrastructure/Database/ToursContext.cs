using Microsoft.EntityFrameworkCore;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Infrastructure.Database.Configurations;

namespace TouristApp.Tours.Infrastructure.Database;

/// <summary>
/// EF Core DbContext za Tours modul.
/// </summary>
public class ToursContext : DbContext
{
    public DbSet<Tour> Tours { get; set; } = null!;

    public ToursContext(DbContextOptions<ToursContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tours");
        modelBuilder.ApplyConfiguration(new TourConfiguration());
        modelBuilder.ApplyConfiguration(new KeyPointConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}