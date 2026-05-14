using Microsoft.EntityFrameworkCore;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Infrastructure.Database.Repositories;

internal class TourRepository : ITourRepository
{
    private readonly ToursContext _context;

    public TourRepository(ToursContext context)
    {
        _context = context;
    }

    public void Add(Tour tour)
    {
        _context.Tours.Add(tour);
        _context.SaveChanges();
    }

    public void Update(Tour tour)
    {
        _context.Tours.Update(tour);
        _context.SaveChanges();
    }

    public Tour? GetById(long id)
    {
        return _context.Tours.FirstOrDefault(t => t.Id == id);
    }

    public PagedResult<Tour> GetByAuthorId(
        long authorId,
        int page,
        int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Tours
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.Id);

        var totalCount = query.Count();
        var items = query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PagedResult<Tour>(items, totalCount);
    }

    public PagedResult<Tour> GetActive(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Tours
            .Where(t => t.Status == TouristApp.Tours.Core.Domain.TourStatus.Published)
            .OrderByDescending(t => t.Id);

        var totalCount = query.Count();
        var items = query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PagedResult<Tour>(items, totalCount);
    }

    public void Delete(Tour tour)
    {
        _context.Tours.Remove(tour);
        _context.SaveChanges();
    }
}