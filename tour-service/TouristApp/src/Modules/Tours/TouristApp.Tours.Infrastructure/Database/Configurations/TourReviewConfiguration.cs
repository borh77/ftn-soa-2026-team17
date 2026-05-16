using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class TourReviewConfiguration : IEntityTypeConfiguration<TourReview>
{
    public void Configure(EntityTypeBuilder<TourReview> builder)
    {
        builder.ToTable("TourReviews", "Tours");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseIdentityAlwaysColumn();

        builder.Property(r => r.TourId).IsRequired();
        builder.Property(r => r.TouristId).IsRequired();
        builder.Property(r => r.TouristUsername).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.VisitedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.Property<List<string>>("_images")
            .HasColumnName("Images")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne<Tour>()
            .WithMany()
            .HasForeignKey(r => r.TourId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.TourId)
            .HasDatabaseName("IX_TourReviews_TourId");
    }
}
