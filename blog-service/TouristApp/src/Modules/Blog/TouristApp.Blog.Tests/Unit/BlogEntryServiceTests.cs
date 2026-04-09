using AutoMapper;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.Core.Domain;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.Blog.Core.Mappers;
using TouristApp.Blog.Core.UseCases;
using Moq;
using Shouldly;
using Xunit;

namespace TouristApp.Blog.Tests.UnitTest;

public class BlogEntryServiceTests
{
    [Fact]
    public void Creates_blog_successfully()
    {
        // Arrange
        var newBlogDto = new BlogEntryDto
        {
            Title = "Test Blog",
            Description = "Test Markdown",
            CreationDate = DateTime.Now,
            Images = new List<string>()
        };

        var repo = new Mock<IBlogEntryRepository>();
        repo.Setup(r => r.Create(It.IsAny<Core.Domain.Blog>()))
            .Returns(new Core.Domain.Blog(newBlogDto.Title, newBlogDto.Description, newBlogDto.CreationDate, newBlogDto.Images));

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<BlogProfile>());
        var mapper = mapperConfig.CreateMapper();

        var service = new BlogEntryService(repo.Object, mapper);

        // Act
        var result = service.Create(newBlogDto);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("Test Blog");
        repo.Verify(r => r.Create(It.IsAny<Core.Domain.Blog>()), Times.Once);
    }
}