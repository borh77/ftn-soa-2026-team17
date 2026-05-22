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

        // If initial keypoints provided in DTO, add them and let server assign ordinals
        if (dto.KeyPoints != null && dto.KeyPoints.Any())
        {
            // Validate initial keypoints: no negative ordinals
            var negative = dto.KeyPoints.Any(k => k.OrdinalNo.HasValue && k.OrdinalNo.Value <= 0);
            if (negative)
                throw new EntityValidationException("Redni broj ključne tačke mora biti pozitivan broj.");

            // Sort keypoints by ordinal (nulls last) so we add them in order without ordinal-too-large errors
            var sortedKps = dto.KeyPoints
                .OrderBy(k => k.OrdinalNo.HasValue ? k.OrdinalNo.Value : int.MaxValue)
                .ToList();

            // Add keypoints respecting provided ordinals (insert at positions)
            foreach (var kpDto in sortedKps)
            {
                var ordinal = kpDto.OrdinalNo.HasValue ? kpDto.OrdinalNo.Value : tour.KeyPoints.Count + 1;
                var kp = new KeyPoint(
                    ordinal,
                    kpDto.Name,
                    kpDto.Description,
                    kpDto.SecretText,
                    kpDto.ImageUrl,
                    kpDto.Latitude,
                    kpDto.Longitude
                );
                tour.AddKeyPoint(kp);
            }
        }

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

    public PagedResult<TourResponseDto> GetActive(int page, int pageSize)
    {
        var result = _tourRepository.GetActive(page, pageSize);
        return new PagedResult<TourResponseDto>(
            result.Results.Select(_mapper.Map<TourResponseDto>).ToList(),
            result.TotalCount);
    }

    public void AddKeyPoint(long tourId, long authorId, KeyPointDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može dodati ključnu tačku.");

        // Determine ordinal: use client-provided if present, otherwise append
        var ordinal = dto.OrdinalNo.HasValue ? dto.OrdinalNo.Value : tour.KeyPoints.Count + 1;
        if (ordinal <= 0)
            throw new EntityValidationException("Redni broj ključne tačke mora biti pozitivan broj.");

        var keyPoint = new KeyPoint(
            ordinal,
            dto.Name,
            dto.Description,
            dto.SecretText,
            dto.ImageUrl,
            dto.Latitude,
            dto.Longitude
        );

        tour.AddKeyPoint(keyPoint);
        _tourRepository.Update(tour);
    }

    public void UpdateKeyPoint(long tourId, int ordinalNo, long authorId, KeyPointDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može ažurirati ključnu tačku.");

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

    public void RemoveKeyPoint(long tourId, int ordinalNo, long authorId)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može obrisati ključnu tačku.");

        tour.RemoveKeyPoint(ordinalNo);
        _tourRepository.Update(tour);
    }

    public void Publish(long tourId, long authorId)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može objaviti turu.");

        tour.Publish();
        _tourRepository.Update(tour);
    }

    public void Archive(long tourId, long authorId)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može arhivirati turu.");

        tour.Archive();
        _tourRepository.Update(tour);
    }

    public void Delete(long tourId, long authorId)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može obrisati turu.");

        if (tour.Status != TourStatus.Draft)
            throw new EntityValidationException("Turu je moguće obrisati samo dok je u stanju Draft.");

        _tourRepository.Delete(tour);
    }

    public void Update(long tourId, long authorId, UpdateTourDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tura sa ID-om {tourId} nije pronađena.");

        if (tour.AuthorId != authorId)
            throw new EntityValidationException("Samo autor ture može izmeniti turu.");

        if (tour.Status != TourStatus.Draft)
            throw new EntityValidationException("Turu je moguće menjati samo dok je u stanju Draft.");

        if (!Enum.TryParse<TourDifficulty>(dto.Difficulty, true, out var difficulty))
            throw new EntityValidationException("Tezina ture mora biti jedna od vrednosti: Easy, Medium, Hard.");

        // Validate keypoints in update payload (if any): no negative ordinals, no duplicate ordinals
        if (dto.KeyPoints != null && dto.KeyPoints.Any())
        {
            var negative = dto.KeyPoints.Any(k => k.OrdinalNo.HasValue && k.OrdinalNo.Value <= 0);
            if (negative)
                throw new EntityValidationException("Redni broj ključne tačke mora biti pozitivan broj.");

            var duplicates = dto.KeyPoints.Where(k => k.OrdinalNo.HasValue).Select(k => k.OrdinalNo!.Value).GroupBy(x => x).Any(g => g.Count() > 1);
            if (duplicates)
                throw new EntityValidationException("Duplikat rednih brojeva u listi ključnih tačaka nije dozvoljen.");
        }

        tour.UpdateDetails(dto.Name, dto.Description, difficulty, dto.Tags ?? new List<string>(), dto.Price);

        // Process keypoints if provided: respect ordinals or append
        if (dto.KeyPoints != null && dto.KeyPoints.Any())
        {
            foreach (var kpDto in dto.KeyPoints)
            {
                var ordinal = kpDto.OrdinalNo.HasValue ? kpDto.OrdinalNo.Value : tour.KeyPoints.Count + 1;
                var kp = new KeyPoint(
                    ordinal,
                    kpDto.Name,
                    kpDto.Description,
                    kpDto.SecretText,
                    kpDto.ImageUrl,
                    kpDto.Latitude,
                    kpDto.Longitude
                );
                tour.AddKeyPoint(kp);
            }
        }

        _tourRepository.Update(tour);
    }
}