using TouristApp.API.Controllers.Tourist;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace TouristApp.Blog.Tests.Integration;

[Collection("Sequential")]
public class BlogCommandTests : BaseBlogIntegrationTest
{
    public BlogCommandTests(BlogTestFactory factory) : base(factory) { }

    [Fact]
    public void Creates_successfully()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);
        var newEntity = new BlogEntryDto
        {
            Title = "Novi Blog",
            Description = "Neki **markdown** opis",
            CreationDate = DateTime.UtcNow,
            Images = new List<string> { "slika.jpg" }
        };

        // Act
        var result = (ObjectResult)controller.Create(newEntity).Result;

        // Assert - Provera status koda i rezultata
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);

        var resultValue = result.Value as BlogEntryDto;
        resultValue.ShouldNotBeNull();
        resultValue.Id.ShouldNotBe(0);
        resultValue.Title.ShouldBe(newEntity.Title);
    }

    [Fact]
    public void Fails_due_to_invalid_data()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);
        var invalidEntity = new BlogEntryDto
        {
            Title = "", // Nevalidno
            Description = "Opis"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => controller.Create(invalidEntity));
    }

    private static BlogEntryController CreateController(IServiceScope scope)
    {
        return new BlogEntryController(scope.ServiceProvider.GetRequiredService<IBlogEntryService>())
        {
            ControllerContext = BuildContext("-1") // Postavlja testni identitet korisnika
        };
    }
}