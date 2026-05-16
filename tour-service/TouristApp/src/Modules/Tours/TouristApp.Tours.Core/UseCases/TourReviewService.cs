using AutoMapper;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;

namespace TouristApp.Tours.Core.UseCases;

public class TourReviewService : ITourReviewService
{
    private readonly ITourRepository _tourRepository;
    private readonly ITourReviewRepository _tourReviewRepository;
    private readonly IMapper _mapper;

    public TourReviewService(
        ITourRepository tourRepository,
        ITourReviewRepository tourReviewRepository,
        IMapper mapper)
    {
        _tourRepository = tourRepository;
        _tourReviewRepository = tourReviewRepository;
        _mapper = mapper;
    }

    public TourReviewDto Create(long tourId, long touristId, string touristUsername, CreateTourReviewDto dto)
    {
        var tour = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tour with id {tourId} was not found.");

        if (tour.Status != TourStatus.Published)
            throw new EntityValidationException("Only published tours can be reviewed.");

        var review = TourReview.Create(
            tourId,
            touristId,
            touristUsername,
            dto.Rating,
            dto.Comment,
            dto.VisitedAt,
            dto.Images);

        _tourReviewRepository.Add(review);

        return _mapper.Map<TourReviewDto>(review);
    }

    public PagedResult<TourReviewDto> GetByTour(long tourId, int page, int pageSize)
    {
        _ = _tourRepository.GetById(tourId)
            ?? throw new EntityValidationException($"Tour with id {tourId} was not found.");

        var result = _tourReviewRepository.GetByTourId(tourId, page, pageSize);
        return new PagedResult<TourReviewDto>(
            result.Results.Select(_mapper.Map<TourReviewDto>).ToList(),
            result.TotalCount);
    }
}
