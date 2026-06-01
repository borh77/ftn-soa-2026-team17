using TouristApp.Tours.API.Dtos;

namespace TouristApp.Tours.API.Public;

public interface ITourExecutionService
{
    TourExecutionDto Start(long touristId, long tourId, StartTourExecutionDto dto);
    KeyPointProximityResultDto CheckKeyPointProximity(long touristId, long executionId, CheckKeyPointProximityDto dto);
    TourExecutionDto Complete(long touristId, long executionId);
    TourExecutionDto Abandon(long touristId, long executionId);
}
