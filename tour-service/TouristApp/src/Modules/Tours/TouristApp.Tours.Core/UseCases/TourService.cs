using AutoMapper;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Core.Domain.Repositories;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Core.UseCases;

public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly IMapper _mapper;

    public TourService(ITourRepository tourRepository, IMapper mapper)
    {
        _tourRepository = tourRepository;
        _mapper = mapper;
    }

    public TourResponseDto Create(long authorId, CreateTourDto dto)
    {
        if (!Enum.TryParse<TourDifficulty>(dto.Difficulty, true, out var difficulty))
            throw new EntityValidationException("Tezina ture mora biti jedna od vrednosti: Easy, Medium, Hard.");

       
        var tour = Tour.Create(
            authorId,
            dto.Name,
            dto.Description,
            difficulty,
            dto.Tags ?? new List<string>()
        );

        _tourRepository.Add(tour);

        return _mapper.Map<TourResponseDto>(tour);
    }

    public PagedResult<TourResponseDto> GetByAuthor(
        long authorId,
        int page,
        int pageSize)
    {
        var result = _tourRepository.GetByAuthorId(authorId, page, pageSize);
        return new PagedResult<TourResponseDto>(
            result.Results.Select(_mapper.Map<TourResponseDto>).ToList(),
            result.TotalCount);
    }

    public void AddKeyPoint(long tourId, KeyPointDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        var keyPoint = _mapper.Map<KeyPoint>(dto);
        tour.AddKeyPoint(keyPoint);
        _tourRepository.Update(tour);
    }

    public void UpdateKeyPoint(long tourId, int ordinalNo, KeyPointDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        var update = new KeyPointUpdate(
            dto.Name,
            dto.Description,
            dto.SecretText,
            dto.ImageUrl,
            dto.Latitude,
            dto.Longitude
        );
        tour.UpdateKeyPoint(ordinalNo, update);
        _tourRepository.Update(tour);
    }

    public void RemoveKeyPoint(long tourId, int ordinalNo)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        tour.RemoveKeyPoint(ordinalNo);
        _tourRepository.Update(tour);
    }
}