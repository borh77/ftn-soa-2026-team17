// TouristApp.Blog.Tests/Integration/BlogCommentCommandTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TouristApp.API.Controllers.Tourist;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using Xunit;

namespace TouristApp.Blog.Tests.Integration;

[Collection("Sequential")]
public class BlogCommentCommandTests : BaseBlogIntegrationTest
{
    public BlogCommentCommandTests(BlogTestFactory factory) : base(factory) { }

    // --- AddComment ---

    [Fact]
    public void AddComment_creates_and_returns_comment()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "101");

        var result = (ObjectResult)controller.AddComment(
            blogId: 1,
            new CreateCommentDto { Text = "Integracioni test komentar" }).Result;

        result.StatusCode.ShouldBe(200);
        var dto = result.Value.ShouldBeOfType<CommentDto>();
        dto.Id.ShouldNotBe(0);
        dto.AuthorId.ShouldBe(101);
        dto.Text.ShouldBe("Integracioni test komentar");
        dto.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        dto.LastModifiedAt.ShouldBeNull();
    }

    [Fact]
    public void AddComment_fails_when_blog_does_not_exist()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "101");

        Should.Throw<KeyNotFoundException>(() =>
            controller.AddComment(blogId: 9999,
                new CreateCommentDto { Text = "Nepostojeći blog" }));
    }

    [Fact]
    public void AddComment_fails_when_text_is_empty()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "101");

        Should.Throw<ArgumentException>(() =>
            controller.AddComment(blogId: 1, new CreateCommentDto { Text = "" }));
    }

    // --- UpdateComment ---

    [Fact]
    public void UpdateComment_author_can_edit_own_comment()
    {
        using var scope = Factory.Services.CreateScope();
        // Komentar id=100 u seed podacima je autor 101
        var controller = CreateController(scope, userId: "101");

        var result = (ObjectResult)controller.UpdateComment(
            blogId: 1, commentId: 100,
            new UpdateCommentDto { Text = "Izmenjen u testu" }).Result;

        result.StatusCode.ShouldBe(200);
        var dto = result.Value.ShouldBeOfType<CommentDto>();
        dto.Text.ShouldBe("Izmenjen u testu");
        dto.LastModifiedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateComment_non_author_gets_unauthorized()
    {
        using var scope = Factory.Services.CreateScope();
        // Komentar id=100 je vlasništvo autora 101; ovde smo ulogovani kao 999
        var controller = CreateController(scope, userId: "999");

        Should.Throw<UnauthorizedAccessException>(() =>
            controller.UpdateComment(blogId: 1, commentId: 100,
                new UpdateCommentDto { Text = "Hack" }));
    }

    [Fact]
    public void UpdateComment_fails_when_comment_does_not_exist()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "101");

        Should.Throw<KeyNotFoundException>(() =>
            controller.UpdateComment(blogId: 1, commentId: 9999,
                new UpdateCommentDto { Text = "Ne postoji" }));
    }

    // --- DeleteComment ---

    [Fact]
    public void DeleteComment_author_can_delete_own_comment()
    {
        using var scope = Factory.Services.CreateScope();
        // Komentar id=101 je vlasništvo autora 102
        var controller = CreateController(scope, userId: "102");

        var result = (NoContentResult)controller.DeleteComment(blogId: 1, commentId: 101);

        result.StatusCode.ShouldBe(204);
    }

    [Fact]
    public void DeleteComment_non_author_gets_unauthorized()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "999");

        Should.Throw<UnauthorizedAccessException>(() =>
            controller.DeleteComment(blogId: 1, commentId: 100));
    }

    private static BlogEntryController CreateController(IServiceScope scope, string userId) =>
        new(scope.ServiceProvider.GetRequiredService<IBlogEntryService>())
        {
            ControllerContext = BuildContext(userId)
        };
}