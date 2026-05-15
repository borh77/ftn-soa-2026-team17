namespace TouristApp.Tours.API.Dtos;

public record TouristPositionDto(
    double Latitude,
    double Longitude,
    DateTime UpdatedAt
);