namespace TouristApp.Tours.API.Dtos;

public record UpdateTourDto(
    string Name,
    string Description,
    string Difficulty,
    List<string>? Tags,
    decimal Price
);
