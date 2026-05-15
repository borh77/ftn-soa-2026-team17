using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.BuildingBlocks.Core.UseCases;


namespace TouristApp.Blog.Core.UseCases;

public class BlogEntryService : IBlogEntryService
{
    private readonly IBlogEntryRepository _blogRepository;
    private readonly IMongoCommentRepository _mongoCommentRepository;
    private readonly IMapper _mapper;

    public BlogEntryService(
        IBlogEntryRepository blogRepository,
        IMongoCommentRepository mongoCommentRepository,
        IMapper mapper)
    {
        _blogRepository = blogRepository;
        _mongoCommentRepository = mongoCommentRepository;
        _mapper = mapper;
    }

    public BlogEntryDto Create(BlogEntryDto blogDto)
    {
        var blog = _mapper.Map<Domain.Blog>(blogDto);
        return _mapper.Map<BlogEntryDto>(_blogRepository.Create(blog));
    }

    public PagedResult<BlogEntryDto> GetPaged(int page, int pageSize)
    {
        var result = _blogRepository.GetPaged(page, pageSize);
        return new PagedResult<BlogEntryDto>(
            result.Results.Select(b => _mapper.Map<BlogEntryDto>(b)).ToList(),
            result.TotalCount);
    }

    public CommentDto AddComment(long blogId, long authorId, string text)
    {
        var blog = GetBlogOrThrow(blogId);
        var comment = blog.AddComment(authorId, text);
        _blogRepository.Save(blog);
        _mongoCommentRepository.Save(comment);
        return _mapper.Map<CommentDto>(comment);
    }

    public CommentDto UpdateComment(long blogId, long commentId, long requesterId, string newText)
    {
        var blog = GetBlogOrThrow(blogId);
        blog.UpdateComment(commentId, requesterId, newText);
        _blogRepository.Save(blog);
        var updated = blog.Comments.First(c => c.Id == commentId);
        _mongoCommentRepository.Update(updated);

        return _mapper.Map<CommentDto>(updated);
    }

    public void DeleteComment(long blogId, long commentId, long requesterId)
    {
        var blog = GetBlogOrThrow(blogId);
        blog.DeleteComment(commentId, requesterId);
        _blogRepository.Save(blog);
        _mongoCommentRepository.Delete(commentId);
    }

    private Domain.Blog GetBlogOrThrow(long blogId) =>
        _blogRepository.GetById(blogId)
        ?? throw new KeyNotFoundException($"Blog {blogId} not found.");


    public void LikeBlog(long blogId, long userId)
    {
        var blog = GetBlogOrThrow(blogId);
        blog.AddLike(userId);
        _blogRepository.Save(blog);
    }

    public void UnlikeBlog(long blogId, long userId)
    {
        var blog = GetBlogOrThrow(blogId);
        blog.RemoveLike(userId);
        _blogRepository.Save(blog);
    }
}