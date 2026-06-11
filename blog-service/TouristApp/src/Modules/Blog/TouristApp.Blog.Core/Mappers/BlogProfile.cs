using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;

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
            .ConstructUsing(src => new Domain.Blog(
                src.AuthorId,
                src.Title,
                src.Description,
                src.CreationDate == default ? DateTime.UtcNow : src.CreationDate,
                src.Images
            ))
            .ForMember(d => d.Comments, o => o.Ignore())
            .ForMember(d => d.Likes, o => o.Ignore());

        CreateMap<Comment, CommentDto>();
    }
}