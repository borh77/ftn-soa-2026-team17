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

    public DbSet<TourReview> TourReviews { get; set; } = null!;

    public DbSet<TouristPosition> TouristPositions { get; set; } = null!;

    public DbSet<TourExecution> TourExecutions { get; set; } = null!;

    public ToursContext(DbContextOptions<ToursContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tours");
        modelBuilder.Ignore<TourTravelTime>();
        modelBuilder.ApplyConfiguration(new TourConfiguration());
        modelBuilder.ApplyConfiguration(new KeyPointConfiguration());
        modelBuilder.ApplyConfiguration(new TourReviewConfiguration());
        modelBuilder.ApplyConfiguration(new TouristPositionConfiguration());
        modelBuilder.ApplyConfiguration(new TourExecutionConfiguration());
        modelBuilder.ApplyConfiguration(new CompletedKeyPointConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
