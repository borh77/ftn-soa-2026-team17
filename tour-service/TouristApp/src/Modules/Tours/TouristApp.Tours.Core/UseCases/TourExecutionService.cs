using AutoMapper;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Core.UseCases;

public class TourExecutionService : ITourExecutionService
{
    private const double KeyPointProximityThresholdMeters = 50d;
    private readonly ITourRepository _tourRepository;
    private readonly ITourExecutionRepository _tourExecutionRepository;
    private readonly IMapper _mapper;

    public TourExecutionService(
        ITourRepository tourRepository,
        ITourExecutionRepository tourExecutionRepository,
        IMapper mapper)
    {
        _tourRepository = tourRepository;
        _tourExecutionRepository = tourExecutionRepository;
        _mapper = mapper;
    }

    public TourExecutionDto Start(long touristId, long tourId, StartTourExecutionDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (_tourExecutionRepository.GetActive(touristId, tourId) != null)
            throw new EntityValidationException("Turista već ima aktivnu sesiju za ovu turu.");

        var execution = TourExecution.Start(tour, touristId, dto.Latitude, dto.Longitude);
        _tourExecutionRepository.Add(execution);

        return _mapper.Map<TourExecutionDto>(execution);
    }

    public KeyPointProximityResultDto CheckKeyPointProximity(long touristId, long executionId, CheckKeyPointProximityDto dto)
    {
        var execution = GetOwnedExecution(touristId, executionId);
        var tour = _tourRepository.GetById(execution.TourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {execution.TourId} nije pronađena.");

        var reachedKeyPoint = execution.CheckKeyPointProximity(tour, dto.Latitude, dto.Longitude, KeyPointProximityThresholdMeters);
        _tourExecutionRepository.Update(execution);

        return new KeyPointProximityResultDto(
            reachedKeyPoint != null,
            reachedKeyPoint?.KeyPointOrdinalNo,
            execution.LastActivity,
            _mapper.Map<TourExecutionDto>(execution));
    }

    public TourExecutionDto Complete(long touristId, long executionId)
    {
        var execution = GetOwnedExecution(touristId, executionId);
        execution.Complete();
        _tourExecutionRepository.Update(execution);

        return _mapper.Map<TourExecutionDto>(execution);
    }

    public TourExecutionDto Abandon(long touristId, long executionId)
    {
        var execution = GetOwnedExecution(touristId, executionId);
        execution.Abandon();
        _tourExecutionRepository.Update(execution);

        return _mapper.Map<TourExecutionDto>(execution);
    }

    private TourExecution GetOwnedExecution(long touristId, long executionId)
    {
        var execution = _tourExecutionRepository.GetById(executionId)
            ?? throw new EntityValidationException($"Sesija ture sa ID-om {executionId} nije pronađena.");

        if (execution.TouristId != touristId)
            throw new EntityValidationException("Turista može upravljati samo sopstvenom sesijom ture.");

        return execution;
    }
}
