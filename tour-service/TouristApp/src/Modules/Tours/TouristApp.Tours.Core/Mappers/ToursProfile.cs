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
                ctx.Mapper.Map<IReadOnlyList<KeyPointDto>>(src.KeyPoints)
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

        CreateMap<KeyPointDto, KeyPoint>()
            .ConstructUsing(src => new KeyPoint(
                src.OrdinalNo,
                src.Name,
                src.Description,
                src.SecretText,
                src.ImageUrl,
                src.Latitude,
                src.Longitude
            ));
    }
}