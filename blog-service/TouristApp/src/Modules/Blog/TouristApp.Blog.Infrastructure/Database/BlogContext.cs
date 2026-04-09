using Microsoft.EntityFrameworkCore;

namespace TouristApp.Blog.Infrastructure.Database;

/// <summary>
/// EF Core DbContext for the Blog module.
/// Add DbSets and entity configurations
/// </summary>
public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("blog"); // might change in the future, but for now this is fine
        base.OnModelCreating(modelBuilder);
    }
}
