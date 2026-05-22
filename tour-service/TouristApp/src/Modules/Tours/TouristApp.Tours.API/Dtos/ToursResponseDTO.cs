namespace TouristApp.Tours.API.Dtos;

public record TourResponseDto(
    long Id,
    long AuthorId,
    string Name,
    string Description,
    string Difficulty,
    IReadOnlyList<string> Tags,
    string Status,
    decimal Price,
    decimal RouteLengthKm,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    IReadOnlyList<TourTravelTimeDto> TravelTimes,
    IReadOnlyList<KeyPointDto> KeyPoints
);