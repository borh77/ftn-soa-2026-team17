using TouristApp.Blog.API.Dtos;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Blog.API.Public;

public interface IBlogEntryService
{
    BlogEntryDto Create(BlogEntryDto blogDto);
    PagedResult<BlogEntryDto> GetPaged(int page, int pageSize);
}