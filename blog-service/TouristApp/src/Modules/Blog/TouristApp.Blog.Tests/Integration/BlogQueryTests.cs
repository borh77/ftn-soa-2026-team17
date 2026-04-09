using TouristApp.API.Controllers.Tourist;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.BuildingBlocks.Core.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TouristApp.Blog.Tests;
using TouristApp.BuildingBlocks.Core.UseCases;
using Xunit;

namespace TouristApp.Blog.Tests.Integration;
[Collection("Sequential")]
public class BlogQueryTests : BaseBlogIntegrationTest
{
    public BlogQueryTests(BlogTestFactory factory) : base(factory) { }

    [Fact]
    public void Retrieves_all_paged()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        // Act
        var result = ((ObjectResult)controller.GetAll(1, 10).Result)?.Value as PagedResult<BlogEntryDto>;

        // Assert
        result.ShouldNotBeNull();
        result.Results.Count.ShouldBeGreaterThan(0); 
    }

    private static BlogEntryController CreateController(IServiceScope scope)
    {
        return new BlogEntryController(scope.ServiceProvider.GetRequiredService<IBlogEntryService>())
        {
            ControllerContext = BuildContext("-1")
        };
    }
}