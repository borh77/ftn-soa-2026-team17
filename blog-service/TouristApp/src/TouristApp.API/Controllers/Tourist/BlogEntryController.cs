using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.BuildingBlocks.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TouristApp.API.Controllers.Tourist;

[Authorize(Policy = "touristPolicy")]
[Route("api/tourist/blog")]
[ApiController]
public class BlogEntryController : ControllerBase
{
    private readonly IBlogEntryService _blogService;

    public BlogEntryController(IBlogEntryService blogService)
    {
        _blogService = blogService;
    }

    [HttpPost]
    public ActionResult<BlogEntryDto> Create([FromBody] BlogEntryDto blog)
    {
        return Ok(_blogService.Create(blog));
    }

    [HttpGet]
    public ActionResult<PagedResult<BlogEntryDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
    {
        return Ok(_blogService.GetPaged(page, pageSize));
    }
}