using TouristApp.Tours.API.Dtos;

namespace TouristApp.Tours.API.Public;

public interface ITouristPositionService
{
    TouristPositionDto? GetForTourist(long touristId);
    TouristPositionDto SaveForTourist(long touristId, UpdateTouristPositionDto dto);
}