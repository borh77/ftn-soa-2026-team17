namespace TouristApp.Tours.API.Dtos;

public record TourTravelTimeDto(
    TransportType TransportType,
    int Minutes);