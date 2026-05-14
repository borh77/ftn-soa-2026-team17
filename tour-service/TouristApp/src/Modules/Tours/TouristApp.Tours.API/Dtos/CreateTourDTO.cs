namespace TouristApp.Tours.API.Dtos;

public record CreateTourDto(
    string Name,
    string Description,
    string Difficulty,
    List<string> Tags
);