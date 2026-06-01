using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class TourExecutionConfiguration : IEntityTypeConfiguration<TourExecution>
{
    public void Configure(EntityTypeBuilder<TourExecution> builder)
    {
        builder.ToTable("TourExecutions");

        builder.HasKey(execution => execution.Id);

        builder.Property(execution => execution.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(execution => execution.TourId)
            .IsRequired();

        builder.Property(execution => execution.TouristId)
            .IsRequired();

        builder.Property(execution => execution.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(execution => execution.StartedAt)
            .IsRequired();

        builder.Property(execution => execution.LastActivity)
            .IsRequired();

        builder.Property(execution => execution.StartedLatitude)
            .IsRequired();

        builder.Property(execution => execution.StartedLongitude)
            .IsRequired();

        builder.HasMany(execution => execution.CompletedKeyPoints)
            .WithOne()
            .HasForeignKey(point => point.TourExecutionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(execution => execution.CompletedKeyPoints)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(execution => new { execution.TouristId, execution.TourId, execution.Status });
    }
}
