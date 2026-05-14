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
    IReadOnlyList<KeyPointDto> KeyPoints
);