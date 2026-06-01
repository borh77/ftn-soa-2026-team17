namespace TouristApp.Tours.API.Dtos;

public record KeyPointProximityResultDto(
    bool Reached,
    int? KeyPointOrdinalNo,
    DateTime LastActivity,
    TourExecutionDto Execution
);
