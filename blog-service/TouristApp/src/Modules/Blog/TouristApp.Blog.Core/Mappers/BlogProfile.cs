using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace TouristApp.Blog.Core.Mappers;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Domain.Blog, BlogEntryDto>()
            .ForMember(d => d.Comments, o => o.MapFrom(s => s.Comments))
            .ForMember(d => d.LikeCount, o => o.MapFrom(s => s.LikeCount))
            .ForMember(d => d.IsLikedByCurrentUser, o => o.Ignore());

        CreateMap<BlogEntryDto, Domain.Blog>()
            .ForMember(d => d.Comments, o => o.Ignore())
            .ForMember(d => d.Likes, o => o.Ignore());

        CreateMap<Comment, CommentDto>();
    }
}