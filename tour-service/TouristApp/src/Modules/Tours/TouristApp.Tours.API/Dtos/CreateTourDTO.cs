namespace TouristApp.Tours.API.Dtos;

public record CreateTourDto(
    string Name,
    string Description,
    string Difficulty,
    List<string> Tags,
    List<TourTravelTimeDto>? TravelTimes = null,
    List<KeyPointDto>? KeyPoints = null,
    decimal? RouteLengthKm = null
);