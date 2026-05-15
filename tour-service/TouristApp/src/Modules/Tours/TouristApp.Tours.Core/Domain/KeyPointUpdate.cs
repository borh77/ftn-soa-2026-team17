namespace TouristApp.Tours.Core.Domain;

public record KeyPointUpdate(
    string? Name,
    string? Description,
    string? SecretText,
    string? ImageUrl,
    double Latitude,
    double Longitude);
