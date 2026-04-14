using Microsoft.EntityFrameworkCore;
using TouristApp.Blog.Core.Domain;

namespace TouristApp.Blog.Infrastructure.Database;

public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options) { }

    public DbSet<Core.Domain.Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("blog");

        modelBuilder.Entity<Core.Domain.Blog>()
            .Property(b => b.Images)
            .HasColumnType("jsonb");

        // Comment je owned entitet — živi u istoj tabeli ili zasebnoj
        modelBuilder.Entity<Core.Domain.Blog>()
            .HasMany(b => b.Comments)
            .WithOne()
            .HasForeignKey(c => c.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .ToTable("comments");

        modelBuilder.Entity<BlogLike>()
    .ToTable("blog_likes");

        modelBuilder.Entity<Core.Domain.Blog>()
            .HasMany(b => b.Likes)
            .WithOne()
            .HasForeignKey(l => l.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);


    }
}