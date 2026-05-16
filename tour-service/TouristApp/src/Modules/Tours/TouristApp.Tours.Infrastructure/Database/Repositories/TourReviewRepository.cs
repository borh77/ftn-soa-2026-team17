using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Infrastructure.Database.Repositories;

internal class TourReviewRepository : ITourReviewRepository
{
    private readonly ToursContext _context;

    public TourReviewRepository(ToursContext context)
    {
        _context = context;
    }

    public void Add(TourReview review)
    {
        _context.TourReviews.Add(review);
        _context.SaveChanges();
    }

    public PagedResult<TourReview> GetByTourId(long tourId, int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TourReviews
            .Where(r => r.TourId == tourId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = query.Count();
        var items = query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PagedResult<TourReview>(items, totalCount);
    }
}
