using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Infrastructure.Database.Configurations;

internal class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.ToTable("Tours");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(t => t.AuthorId)
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(t => t.Difficulty)
            .IsRequired()
            .HasConversion<string>();

        // Tagovi se čuvaju kao JSON niz stringova
        builder.Property<List<string>>("_tags")
            .HasColumnName("Tags")
            .HasColumnType("jsonb")
            .IsRequired();

        // KeyPoints su regularna entitetska kolekcija u zasebnoj tabeli
        builder.HasMany(t => t.KeyPoints)
            .WithOne()
            .HasForeignKey("TourId")
            .IsRequired();

        // Koristimo field-backed pristup za kolekciju preko privatnog polja
        builder.Navigation(t => t.KeyPoints).UsePropertyAccessMode(Microsoft.EntityFrameworkCore.PropertyAccessMode.Field);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(TourStatus.Draft);

        builder.Property(t => t.Price)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.HasIndex(t => t.AuthorId)
            .HasDatabaseName("FK_Tours_AuthorId");
    }
}