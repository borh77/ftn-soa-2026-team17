using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Blog.Core.Domain.RepositoryInterfaces;

public interface IBlogEntryRepository
{
    Blog Create(Blog blog);
    PagedResult<Blog> GetPaged(int page, int pageSize);

    
    Blog? GetById(long id);
    Blog Save(Blog blog);
}