using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class KeyPointConfiguration : IEntityTypeConfiguration<KeyPoint>
{
    public void Configure(EntityTypeBuilder<KeyPoint> builder)
    {
        builder.ToTable("KeyPoints", "Tours");

        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).UseIdentityAlwaysColumn();

        builder.Property(k => k.TourId).IsRequired();

        builder.Property(k => k.OrdinalNo).IsRequired();
        builder.Property(k => k.Name).IsRequired().HasMaxLength(200);
        builder.Property(k => k.Description).IsRequired().HasMaxLength(5000);
        builder.Property(k => k.SecretText).IsRequired();
        builder.Property(k => k.ImageUrl).IsRequired();
        builder.Property(k => k.Latitude).IsRequired();
        builder.Property(k => k.Longitude).IsRequired();
    }
}
