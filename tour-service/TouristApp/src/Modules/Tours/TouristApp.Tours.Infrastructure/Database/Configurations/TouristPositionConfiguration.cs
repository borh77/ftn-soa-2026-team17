using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class TouristPositionConfiguration : IEntityTypeConfiguration<TouristPosition>
{
    public void Configure(EntityTypeBuilder<TouristPosition> builder)
    {
        builder.ToTable("TouristPositions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(p => p.TouristId)
            .IsRequired();

        builder.Property(p => p.Latitude)
            .IsRequired();

        builder.Property(p => p.Longitude)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasIndex(p => p.TouristId)
            .IsUnique();
    }
}