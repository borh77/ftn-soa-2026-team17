using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.API.Controllers.Tourist;

[Authorize]

[Route("api/tourist/blog")]
[ApiController]
public class BlogEntryController : ControllerBase
{
    private readonly IBlogEntryService _blogService;

    public BlogEntryController(IBlogEntryService blogService) =>
        _blogService = blogService;

    [HttpPost]
    public ActionResult<BlogEntryDto> Create([FromBody] BlogEntryDto blog)
    {
        var authorId = User.PersonId();
        return Ok(_blogService.Create(blog, authorId));
    }

    [HttpGet]
    public ActionResult<PagedResult<BlogEntryDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize) =>
        Ok(_blogService.GetPaged(page, pageSize));

  

    [HttpPost("{blogId:long}/comments")]
    public ActionResult<CommentDto> AddComment(long blogId, [FromBody] CreateCommentDto dto)
    {
        var authorId = User.PersonId(); // ClaimsPrincipalExtensions
        return Ok(_blogService.AddComment(blogId, authorId, dto.Text));
    }

    [HttpPut("{blogId:long}/comments/{commentId:long}")]
    public ActionResult<CommentDto> UpdateComment(
        long blogId, long commentId, [FromBody] UpdateCommentDto dto)
    {
        var requesterId = User.PersonId();
        return Ok(_blogService.UpdateComment(blogId, commentId, requesterId, dto.Text));
    }

    [HttpDelete("{blogId:long}/comments/{commentId:long}")]
    public ActionResult DeleteComment(long blogId, long commentId)
    {
        var requesterId = User.PersonId();
        _blogService.DeleteComment(blogId, commentId, requesterId);
        return NoContent();
    }



    [HttpPost("{blogId:long}/likes")]
    public ActionResult LikeBlog(long blogId)
    {
        var userId = User.PersonId();
        _blogService.LikeBlog(blogId, userId);
        return Ok();
    }

    [HttpDelete("{blogId:long}/likes")]
    public ActionResult UnlikeBlog(long blogId)
    {
        var userId = User.PersonId();
        _blogService.UnlikeBlog(blogId, userId);
        return NoContent();
    }
}