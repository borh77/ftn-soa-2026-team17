using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;
namespace TouristApp.Blog.Core.Mappers;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Domain.Blog, BlogEntryDto>()
            .ForMember(d => d.Comments, o => o.MapFrom(s => s.Comments));

        CreateMap<BlogEntryDto, Domain.Blog>()
            .ForMember(d => d.Comments, o => o.Ignore());

        CreateMap<Comment, CommentDto>();
    }
}