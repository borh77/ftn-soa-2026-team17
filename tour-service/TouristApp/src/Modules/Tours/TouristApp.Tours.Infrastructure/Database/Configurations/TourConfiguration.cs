using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
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

        var travelTimesConverter = new ValueConverter<List<TourTravelTime>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<TourTravelTime>()
                : JsonSerializer.Deserialize<List<TourTravelTime>>(v, (JsonSerializerOptions?)null) ?? new List<TourTravelTime>());

        var travelTimesComparer = new ValueComparer<List<TourTravelTime>>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => value == null ? new List<TourTravelTime>() : value.ToList());

        builder.Property<List<TourTravelTime>>("_travelTimes")
            .HasColumnName("TravelTimes")
            .HasColumnType("jsonb")
            .HasConversion(travelTimesConverter)
            .IsRequired();

        builder.Property<List<TourTravelTime>>("_travelTimes")
            .Metadata.SetValueComparer(travelTimesComparer);

        builder.HasIndex(t => t.AuthorId)
            .HasDatabaseName("FK_Tours_AuthorId");
    }
}