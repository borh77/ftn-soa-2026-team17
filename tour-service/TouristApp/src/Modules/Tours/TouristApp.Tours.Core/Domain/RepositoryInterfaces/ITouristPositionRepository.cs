using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Core.Domain.Repositories;

public interface ITouristPositionRepository
{
    TouristPosition? GetByTouristId(long touristId);
    void Add(TouristPosition position);
    void Update(TouristPosition position);
}