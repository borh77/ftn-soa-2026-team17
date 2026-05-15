using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Core.UseCases;

public class TouristPositionService : ITouristPositionService
{
    private readonly ITouristPositionRepository _repository;

    public TouristPositionService(ITouristPositionRepository repository)
    {
        _repository = repository;
    }

    public TouristPositionDto? GetForTourist(long touristId)
    {
        var position = _repository.GetByTouristId(touristId);

        if (position == null)
        {
            return null;
        }

        return new TouristPositionDto(
            position.Latitude,
            position.Longitude,
            position.UpdatedAt
        );
    }

    public TouristPositionDto SaveForTourist(long touristId, UpdateTouristPositionDto dto)
    {
        var position = _repository.GetByTouristId(touristId);

        if (position == null)
        {
            position = new TouristPosition(touristId, dto.Latitude, dto.Longitude);
            _repository.Add(position);
        }
        else
        {
            position.Update(dto.Latitude, dto.Longitude);
            _repository.Update(position);
        }

        return new TouristPositionDto(
            position.Latitude,
            position.Longitude,
            position.UpdatedAt
        );
    }
}