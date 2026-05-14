using AutoMapper;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.Core.Domain;

namespace TouristApp.Tours.Core.Mappers;

public class ToursProfile : Profile
{
    public ToursProfile()
    {
        CreateMap<Tour, TourResponseDto>()
            .ConstructUsing(src => new TourResponseDto(
                src.Id,
                src.AuthorId,
                src.Name,
                src.Description,
                src.Difficulty.ToString(),
                src.Tags,
                src.Status.ToString(),
                src.Price
            ));
    }
}