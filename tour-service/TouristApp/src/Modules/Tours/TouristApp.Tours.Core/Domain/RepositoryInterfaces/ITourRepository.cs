using TouristApp.Tours.Core.Domain;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Tours.Core.Domain.Repositories;

public interface ITourRepository
{
    void Add(Tour tour);
    void Update(Tour tour);
    Tour? GetById(long id);
    PagedResult<Tour> GetByAuthorId(
        long authorId,
        int page,
        int pageSize);
}