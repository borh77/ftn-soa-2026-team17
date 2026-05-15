using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Infrastructure.Database.Repositories;

internal class TouristPositionRepository : ITouristPositionRepository
{
    private readonly ToursContext _context;

    public TouristPositionRepository(ToursContext context)
    {
        _context = context;
    }

    public TouristPosition? GetByTouristId(long touristId)
    {
        return _context.TouristPositions
            .FirstOrDefault(p => p.TouristId == touristId);
    }

    public void Add(TouristPosition position)
    {
        _context.TouristPositions.Add(position);
        _context.SaveChanges();
    }

    public void Update(TouristPosition position)
    {
        _context.TouristPositions.Update(position);
        _context.SaveChanges();
    }
}