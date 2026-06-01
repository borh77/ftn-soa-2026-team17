namespace TouristApp.Tours.API.Dtos;

public record TourExecutionDto(
    long Id,
    long TourId,
    long TouristId,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DateTime? AbandonedAt,
    DateTime LastActivity,
    double StartedLatitude,
    double StartedLongitude,
    IReadOnlyList<CompletedKeyPointDto> CompletedKeyPoints
);
