// TouristApp.Blog.Tests/Integration/BlogCommentQueryTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TouristApp.API.Controllers.Tourist;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.BuildingBlocks.Core.UseCases;
using Xunit;

namespace TouristApp.Blog.Tests.Integration;

[Collection("Sequential")]
public class BlogCommentQueryTests : BaseBlogIntegrationTest
{
    public BlogCommentQueryTests(BlogTestFactory factory) : base(factory) { }

    [Fact]
    public void GetAll_returns_blogs_with_comments_included()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var result = ((ObjectResult)controller.GetAll(1, 10).Result)
            ?.Value as PagedResult<BlogEntryDto>;

        result.ShouldNotBeNull();
        result.Results.Count.ShouldBeGreaterThan(0);

        // Blog 1 ima 2 komentara u seed podacima
        var blog1 = result.Results.FirstOrDefault(b => b.Id == 1);
        blog1.ShouldNotBeNull();
        blog1.Comments.ShouldNotBeNull();
        blog1.Comments!.Count.ShouldBe(2);
    }

    [Fact]
    public void GetAll_blog_without_comments_returns_empty_list()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var result = ((ObjectResult)controller.GetAll(1, 10).Result)
            ?.Value as PagedResult<BlogEntryDto>;

        var blog3 = result!.Results.FirstOrDefault(b => b.Id == 3);
        blog3.ShouldNotBeNull();
        blog3.Comments.ShouldNotBeNull();
        blog3.Comments!.ShouldBeEmpty();
    }

    [Fact]
    public void GetAll_comment_has_expected_seed_data()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var result = ((ObjectResult)controller.GetAll(1, 10).Result)
            ?.Value as PagedResult<BlogEntryDto>;

        var blog1 = result!.Results.First(b => b.Id == 1);
        var comment = blog1.Comments!.FirstOrDefault(c => c.Id == 100);
        comment.ShouldNotBeNull();
        comment.AuthorId.ShouldBe(101);
        comment.Text.ShouldBe("Odličan opis!");
        comment.LastModifiedAt.ShouldBeNull();
    }

    [Fact]
    public void GetAll_modified_comment_has_LastModifiedAt_set()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var result = ((ObjectResult)controller.GetAll(1, 10).Result)
            ?.Value as PagedResult<BlogEntryDto>;

        // Komentar id=102 (blog 2) je izmenjen u seed podacima
        var blog2 = result!.Results.First(b => b.Id == 2);
        var comment = blog2.Comments!.First(c => c.Id == 102);
        comment.LastModifiedAt.ShouldNotBeNull();
    }

    private static BlogEntryController CreateController(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IBlogEntryService>())
        {
            ControllerContext = BuildContext("-1")
        };
}