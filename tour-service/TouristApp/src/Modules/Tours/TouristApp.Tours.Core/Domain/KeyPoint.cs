using TouristApp.BuildingBlocks.Core.Domain;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Core.Domain;

public class KeyPoint : ValueObject
{
    public int OrdinalNo { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string SecretText { get; private set; } = null!;
    public string ImageUrl { get; private set; } = null!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // Required by EF Core
    private KeyPoint() { }

    public KeyPoint(
        int ordinalNo,
        string name,
        string description,
        string secretText,
        string imageUrl,
        double latitude,
        double longitude)
    {
        Validate(ordinalNo, name, description, secretText, imageUrl, latitude, longitude);

        OrdinalNo = ordinalNo;
        Name = name;
        Description = description;
        SecretText = secretText;
        ImageUrl = imageUrl;
        Latitude = latitude;
        Longitude = longitude;
    }

    private static void Validate(
        int ordinalNo,
        string name,
        string description,
        string secretText,
        string imageUrl,
        double latitude,
        double longitude)
    {
        if (ordinalNo <= 0)
            throw new EntityValidationException("Redni broj ključne tačke mora biti pozitivan broj.");

        if (string.IsNullOrWhiteSpace(name))
            throw new EntityValidationException("Naziv ključne tačke je obavezan.");

        if (name.Length > 200)
            throw new EntityValidationException("Naziv ključne tačke ne sme biti duži od 200 karaktera.");

        if (string.IsNullOrWhiteSpace(description))
            throw new EntityValidationException("Opis ključne tačke je obavezan.");

        if (description.Length > 5000)
            throw new EntityValidationException("Opis ključne tačke ne sme biti duži od 5000 karaktera.");

        if (string.IsNullOrWhiteSpace(secretText))
            throw new EntityValidationException("Tajni tekst je obavezan.");

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new EntityValidationException("URL slike je obavezan.");

        if (latitude < -90 || latitude > 90)
            throw new EntityValidationException("Geografska širina mora biti između -90 i 90 stepeni.");

        if (longitude < -180 || longitude > 180)
            throw new EntityValidationException("Geografska dužina mora biti između -180 i 180 stepeni.");
    }

    public void UpdateOrdinalNo(int newOrdinal)
    {
        if (newOrdinal <= 0)
            throw new EntityValidationException("Redni broj ključne tačke mora biti pozitivan broj.");
        OrdinalNo = newOrdinal;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OrdinalNo;
        yield return Name;
        yield return Latitude;
        yield return Longitude;
    }
}
