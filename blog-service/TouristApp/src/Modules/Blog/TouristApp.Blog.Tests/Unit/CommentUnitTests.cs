using Shouldly;
using TouristApp.Blog.Core.Domain;
using Xunit;

namespace TouristApp.Blog.Tests.Unit;

public class CommentUnitTests
{

    [Fact]
    public void Creates_comment_successfully()
    {
        var blog = CreateBlog();

        var comment = blog.AddComment(authorId: 42, text: "Sjajan tekst!");

        comment.AuthorId.ShouldBe(42);
        comment.Text.ShouldBe("Sjajan tekst!");
        comment.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        comment.LastModifiedAt.ShouldBeNull();
        blog.Comments.Count.ShouldBe(1);
    }

    [Fact]
    public void Fails_when_text_is_empty()
    {
        var blog = CreateBlog();
        Should.Throw<ArgumentException>(() => blog.AddComment(42, ""));
    }

    [Fact]
    public void Fails_when_text_is_whitespace()
    {
        var blog = CreateBlog();
        Should.Throw<ArgumentException>(() => blog.AddComment(42, "   "));
    }

    [Fact]
    public void Fails_when_authorId_is_zero()
    {
        var blog = CreateBlog();
        Should.Throw<ArgumentException>(() => blog.AddComment(0, "Tekst"));
    }

   
    [Fact]
    public void Author_can_update_own_comment()
    {
        var blog = CreateBlog();
        var comment = blog.AddComment(authorId: 10, text: "Stari tekst");

        blog.UpdateComment(comment.Id, requesterId: 10, "Novi tekst");

        var updated = blog.Comments.First(c => c.Id == comment.Id);
        updated.Text.ShouldBe("Novi tekst");
        updated.LastModifiedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Update_sets_LastModifiedAt()
    {
        var blog = CreateBlog();
        var before = DateTime.UtcNow;
        var comment = blog.AddComment(10, "Tekst");

        blog.UpdateComment(comment.Id, 10, "Izmena");

        var lastModified = blog.Comments.First().LastModifiedAt;
        lastModified.ShouldNotBeNull();
        lastModified.Value.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Non_author_cannot_update_comment()
    {
        var blog = CreateBlog();
        var comment = blog.AddComment(authorId: 10, text: "Tekst");

        Should.Throw<UnauthorizedAccessException>(
            () => blog.UpdateComment(comment.Id, requesterId: 99, "Hack"));
    }

    [Fact]
    public void Update_fails_when_new_text_is_empty()
    {
        var blog = CreateBlog();
        var comment = blog.AddComment(10, "Tekst");

        Should.Throw<ArgumentException>(
            () => blog.UpdateComment(comment.Id, 10, ""));
    }

    // --- Delete ---

    [Fact]
    public void Author_can_delete_own_comment()
    {
        var blog = CreateBlog();
        var comment = blog.AddComment(authorId: 10, text: "Brišem ovo");

        blog.DeleteComment(comment.Id, requesterId: 10);

        blog.Comments.ShouldBeEmpty();
    }

    [Fact]
    public void Non_author_cannot_delete_comment()
    {
        var blog = CreateBlog();
        var comment = blog.AddComment(authorId: 10, text: "Ne diraj");

        Should.Throw<UnauthorizedAccessException>(
            () => blog.DeleteComment(comment.Id, requesterId: 55));
    }

    [Fact]
    public void Delete_nonexistent_comment_throws()
    {
        var blog = CreateBlog();

        Should.Throw<KeyNotFoundException>(
            () => blog.DeleteComment(commentId: 9999, requesterId: 10));
    }

    [Fact]
    public void Blog_can_hold_multiple_comments()
    {
        var blog = CreateBlog();
        blog.AddComment(1, "Prvi");
        blog.AddComment(2, "Drugi");
        blog.AddComment(3, "Treći");

        blog.Comments.Count.ShouldBe(3);
    }

  

    [Fact]
    public void Delete_removes_only_target_comment()
    {
        var blog = CreateBlog();
        var c1 = blog.AddComment(1, "Ostaje");
        SetId(c1, 1);
        var c2 = blog.AddComment(2, "Briše se");
        SetId(c2, 2);

        blog.DeleteComment(c2.Id, requesterId: 2);

        blog.Comments.Count.ShouldBe(1);
        blog.Comments.First().Id.ShouldBe(c1.Id);
    }

    private static Core.Domain.Blog CreateBlog() =>
        new("Test blog", "Opis", DateTime.UtcNow, null);
    private static void SetId(object entity, long id) =>
        entity.GetType().BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
}