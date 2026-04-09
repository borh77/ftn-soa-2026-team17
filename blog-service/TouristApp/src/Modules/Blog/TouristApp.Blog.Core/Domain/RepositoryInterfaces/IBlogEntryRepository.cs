using TouristApp.Blog.Core.Domain;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Blog.Core.Domain.RepositoryInterfaces;

public interface IBlogEntryRepository
{
    Core.Domain.Blog Create(Core.Domain.Blog blog);
    PagedResult<Core.Domain.Blog> GetPaged(int page, int pageSize);
}