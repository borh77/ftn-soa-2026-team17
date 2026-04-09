using Microsoft.EntityFrameworkCore;
using TouristApp.Blog.Core.Domain;

namespace TouristApp.Blog.Infrastructure.Database;

/// <summary>
/// EF Core DbContext for the Blog module.
/// Add DbSets and entity configurations
/// </summary>
public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options) { }
    public DbSet<Core.Domain.Blog> Blogs { get; set; } 

    
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("blog"); // might change in the future, but for now this is fine
        // Mapiranje liste stringova (Images) u JSON kolonu
        modelBuilder.Entity<Core.Domain.Blog>()
            .Property(b => b.Images)
            .HasColumnType("jsonb");
        base.OnModelCreating(modelBuilder);
    }
}
