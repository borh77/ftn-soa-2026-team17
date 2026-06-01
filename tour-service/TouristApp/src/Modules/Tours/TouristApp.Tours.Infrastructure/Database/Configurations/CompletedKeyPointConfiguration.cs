using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class CompletedKeyPointConfiguration : IEntityTypeConfiguration<CompletedKeyPoint>
{
    public void Configure(EntityTypeBuilder<CompletedKeyPoint> builder)
    {
        builder.ToTable("CompletedKeyPoints");

        builder.HasKey(point => point.Id);

        builder.Property(point => point.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(point => point.KeyPointOrdinalNo)
            .IsRequired();

        builder.Property(point => point.CompletedAt)
            .IsRequired();

        builder.HasIndex(point => new { point.TourExecutionId, point.KeyPointOrdinalNo })
            .IsUnique();
    }
}
