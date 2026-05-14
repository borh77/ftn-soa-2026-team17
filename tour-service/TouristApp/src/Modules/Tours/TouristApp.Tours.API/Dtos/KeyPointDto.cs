namespace TouristApp.Tours.API.Dtos;

public record KeyPointDto(
    int OrdinalNo,
    string Name,
    string Description,
    string SecretText,
    string ImageUrl,
    double Latitude,
    double Longitude
);
