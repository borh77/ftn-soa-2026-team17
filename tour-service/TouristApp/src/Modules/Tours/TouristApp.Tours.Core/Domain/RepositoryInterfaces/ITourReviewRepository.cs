using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Tours.Core.Domain.Repositories;

public interface ITourReviewRepository
{
    void Add(TourReview review);
    PagedResult<TourReview> GetByTourId(long tourId, int page, int pageSize);
}
