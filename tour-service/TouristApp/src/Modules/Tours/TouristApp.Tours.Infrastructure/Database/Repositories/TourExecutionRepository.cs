using Microsoft.EntityFrameworkCore;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Infrastructure.Database.Repositories;

internal class TourExecutionRepository : ITourExecutionRepository
{
    private readonly ToursContext _context;

    public TourExecutionRepository(ToursContext context)
    {
        _context = context;
    }

    public void Add(TourExecution tourExecution)
    {
        _context.TourExecutions.Add(tourExecution);
        _context.SaveChanges();
    }

    public void Update(TourExecution tourExecution)
    {
        _context.TourExecutions.Update(tourExecution);
        _context.SaveChanges();
    }

    public TourExecution? GetById(long id)
    {
        return _context.TourExecutions
            .Include(execution => execution.CompletedKeyPoints)
            .FirstOrDefault(execution => execution.Id == id);
    }

    public TourExecution? GetActive(long touristId, long tourId)
    {
        return _context.TourExecutions
            .Include(execution => execution.CompletedKeyPoints)
            .FirstOrDefault(execution =>
                execution.TouristId == touristId &&
                execution.TourId == tourId &&
                execution.Status == TourExecutionStatus.Active);
    }
}
