using TouristApp.Tours.Core.Domain;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Tours.Core.Domain.Repositories;

public interface ITourRepository
{
    void Add(Tour tour);
    PagedResult<Tour> GetByAuthorId(
        long authorId,
        int page,
        int pageSize);
}