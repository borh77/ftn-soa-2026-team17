namespace TouristApp.Tours.API.Dtos;

public record CompletedKeyPointDto(
    int KeyPointOrdinalNo,
    DateTime CompletedAt
);
