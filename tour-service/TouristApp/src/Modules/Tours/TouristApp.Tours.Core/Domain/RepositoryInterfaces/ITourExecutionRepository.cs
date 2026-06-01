using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Core.Domain.Repositories;

public interface ITourExecutionRepository
{
    void Add(TourExecution tourExecution);
    void Update(TourExecution tourExecution);
    TourExecution? GetById(long id);
    TourExecution? GetActive(long touristId, long tourId);
}
