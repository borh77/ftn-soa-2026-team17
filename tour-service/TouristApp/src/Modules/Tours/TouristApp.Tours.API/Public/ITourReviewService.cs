using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.API.Dtos;

namespace TouristApp.Tours.API.Public;

public interface ITourReviewService
{
    TourReviewDto Create(long tourId, long touristId, string touristUsername, CreateTourReviewDto dto);
    PagedResult<TourReviewDto> GetByTour(long tourId, int page, int pageSize);
}
