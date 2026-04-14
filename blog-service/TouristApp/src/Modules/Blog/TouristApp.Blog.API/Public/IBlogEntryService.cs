using TouristApp.Blog.API.Dtos;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Blog.API.Public;

public interface IBlogEntryService
{
    BlogEntryDto Create(BlogEntryDto blogDto);
    PagedResult<BlogEntryDto> GetPaged(int page, int pageSize);

    CommentDto AddComment(long blogId, long authorId, string text);
    CommentDto UpdateComment(long blogId, long commentId, long requesterId, string newText);
    void DeleteComment(long blogId, long commentId, long requesterId);

    void LikeBlog(long blogId, long userId);
    void UnlikeBlog(long blogId, long userId);
}