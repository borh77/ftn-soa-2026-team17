using AutoMapper;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Core.Mappers;

public class ToursProfile : Profile
{
    public ToursProfile()
    {
        CreateMap<Tour, TourResponseDto>()
            .ConstructUsing((src, ctx) => new TourResponseDto(
                src.Id,
                src.AuthorId,
                src.Name,
                src.Description,
                src.Difficulty.ToString(),
                src.Tags,
                src.Status.ToString(),
                src.Price,
                ctx.Mapper.Map<IReadOnlyList<TourTravelTimeDto>>(src.TravelTimes),
                ctx.Mapper.Map<IReadOnlyList<KeyPointDto>>(src.KeyPoints)
            ));

        CreateMap<TourTravelTime, TourTravelTimeDto>()
            .ConstructUsing(src => new TourTravelTimeDto(
                (TouristApp.Tours.API.Dtos.TransportType)src.TransportType,
                src.Minutes
            ));

        CreateMap<KeyPoint, KeyPointDto>()
            .ConstructUsing(src => new KeyPointDto(
                src.OrdinalNo,
                src.Name,
                src.Description,
                src.SecretText,
                src.ImageUrl,
                src.Latitude,
                src.Longitude
            ));

        CreateMap<TourReview, TourReviewDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList()));

        // Intentionally do not map KeyPointDto -> KeyPoint globally because
        // server assigns ordinals and construction is handled in service layer.
    }
}
