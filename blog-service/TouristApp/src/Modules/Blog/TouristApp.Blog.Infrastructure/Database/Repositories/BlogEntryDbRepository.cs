using Microsoft.EntityFrameworkCore;
using TouristApp.Blog.Core.Domain;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.BuildingBlocks.Infrastructure.Database;

namespace TouristApp.Blog.Infrastructure.Database.Repositories;

public class BlogEntryDbRepository : IBlogEntryRepository
{
    private readonly BlogContext _dbContext;

    public BlogEntryDbRepository(BlogContext dbContext) => _dbContext = dbContext;

    public Core.Domain.Blog Create(Core.Domain.Blog blog)
    {
        _dbContext.Blogs.Add(blog);
        _dbContext.SaveChanges();
        return blog;
    }

    public PagedResult<Core.Domain.Blog> GetPaged(int page, int pageSize)
    {
        var task = _dbContext.Blogs
            .Include(b => b.Comments)
            .Include(b => b.Likes)
            .GetPaged(page, pageSize);
        task.Wait();
        return task.Result;
    }

    public Core.Domain.Blog? GetById(long id) =>
      _dbContext.Blogs
          .Include(b => b.Comments)
          .Include(b => b.Likes)
          .FirstOrDefault(b => b.Id == id);
    public Core.Domain.Blog Save(Core.Domain.Blog blog)
    {
        _dbContext.Blogs.Update(blog);
        _dbContext.SaveChanges();
        return blog;
    }
}