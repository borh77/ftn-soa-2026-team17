// TouristApp.Blog.Tests/Unit/BlogEntryServiceTests.cs
using AutoMapper;
using Moq;
using Shouldly;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.Blog.Core.Mappers;
using TouristApp.Blog.Core.UseCases;
using Xunit;

namespace TouristApp.Blog.Tests.Unit;

public class BlogEntryServiceTests
{
    private readonly Mock<IBlogEntryRepository> _repoMock;
    private readonly BlogEntryService _service;

    public BlogEntryServiceTests()
    {
        _repoMock = new Mock<IBlogEntryRepository>();
        var mapper = new MapperConfiguration(c => c.AddProfile<BlogProfile>())
            .CreateMapper();
        _service = new BlogEntryService(_repoMock.Object, mapper);
    }

    // --- Create blog ---

    [Fact]
    public void Creates_blog_successfully()
    {
        var dto = new BlogEntryDto { Title = "Test", Description = "Opis", CreationDate = DateTime.Now };
        _repoMock.Setup(r => r.Create(It.IsAny<Core.Domain.Blog>()))
            .Returns((Core.Domain.Blog b) => b);

        var result = _service.Create(dto);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Test");
        _repoMock.Verify(r => r.Create(It.IsAny<Core.Domain.Blog>()), Times.Once);
    }

    // --- AddComment ---

    [Fact]
    public void AddComment_returns_dto_with_correct_data()
    {
        var blog = MakeBlog(id: 1);
        _repoMock.Setup(r => r.GetById(1)).Returns(blog);
        _repoMock.Setup(r => r.Save(blog)).Returns(blog);

        var result = _service.AddComment(blogId: 1, authorId: 42, text: "Super!");

        result.ShouldNotBeNull();
        result.AuthorId.ShouldBe(42);
        result.Text.ShouldBe("Super!");
        result.LastModifiedAt.ShouldBeNull();
        _repoMock.Verify(r => r.Save(blog), Times.Once);
    }

    [Fact]
    public void AddComment_throws_when_blog_not_found()
    {
        _repoMock.Setup(r => r.GetById(It.IsAny<long>())).Returns((Core.Domain.Blog?)null);

        Should.Throw<KeyNotFoundException>(
            () => _service.AddComment(blogId: 999, authorId: 1, text: "Tekst"));
    }

    // --- UpdateComment ---

    [Fact]
    public void UpdateComment_returns_updated_text()
    {
        var blog = MakeBlog(id: 1);
        var comment = blog.AddComment(authorId: 10, text: "Stari");
        _repoMock.Setup(r => r.GetById(1)).Returns(blog);
        _repoMock.Setup(r => r.Save(blog)).Returns(blog);

        var result = _service.UpdateComment(
            blogId: 1, commentId: comment.Id, requesterId: 10, newText: "Novi");

        result.Text.ShouldBe("Novi");
        result.LastModifiedAt.ShouldNotBeNull();
        _repoMock.Verify(r => r.Save(blog), Times.Once);
    }

    [Fact]
    public void UpdateComment_propagates_unauthorized_from_domain()
    {
        var blog = MakeBlog(id: 1);
        var comment = blog.AddComment(authorId: 10, text: "Tekst");
        _repoMock.Setup(r => r.GetById(1)).Returns(blog);

        Should.Throw<UnauthorizedAccessException>(
            () => _service.UpdateComment(1, comment.Id, requesterId: 99, "Hack"));

        _repoMock.Verify(r => r.Save(It.IsAny<Core.Domain.Blog>()), Times.Never);
    }

    // --- DeleteComment ---

    [Fact]
    public void DeleteComment_calls_save_after_removal()
    {
        var blog = MakeBlog(id: 1);
        var comment = blog.AddComment(authorId: 10, text: "Brišem");
        _repoMock.Setup(r => r.GetById(1)).Returns(blog);
        _repoMock.Setup(r => r.Save(blog)).Returns(blog);

        _service.DeleteComment(blogId: 1, commentId: comment.Id, requesterId: 10);

        blog.Comments.ShouldBeEmpty();
        _repoMock.Verify(r => r.Save(blog), Times.Once);
    }

    [Fact]
    public void DeleteComment_propagates_unauthorized_from_domain()
    {
        var blog = MakeBlog(id: 1);
        var comment = blog.AddComment(authorId: 10, text: "Tekst");
        _repoMock.Setup(r => r.GetById(1)).Returns(blog);

        Should.Throw<UnauthorizedAccessException>(
            () => _service.DeleteComment(1, comment.Id, requesterId: 77));

        _repoMock.Verify(r => r.Save(It.IsAny<Core.Domain.Blog>()), Times.Never);
    }

    private static Core.Domain.Blog MakeBlog(long id)
    {
        var blog = new Core.Domain.Blog("Naslov", "Opis", DateTime.UtcNow, null);
        // Postavljamo Id kroz refleksiju jer je EF Core jedini koji to radi u produkciji
        typeof(Core.Domain.Blog).BaseType!
            .GetProperty("Id")!
            .SetValue(blog, id);
        return blog;
    }
}