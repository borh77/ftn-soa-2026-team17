using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;

namespace TouristApp.Blog.Core.Mappers;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Domain.Blog, BlogEntryDto>().ReverseMap();
    }
}