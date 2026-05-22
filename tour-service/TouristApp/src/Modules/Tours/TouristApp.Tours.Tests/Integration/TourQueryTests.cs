using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.API.Controllers;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;

namespace TouristApp.Tours.Tests.Integration;

[Collection("Sequential")]
public class TourQueryTests : BaseToursIntegrationTest
{
    public TourQueryTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void GetByAuthor_returns_only_tours_for_requested_author()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "101");

        var getResult = (OkObjectResult)controller.GetByAuthor(page: 1, pageSize: 10).Result!;
        var tours = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();

        tours.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        tours.Results.All(t => t.AuthorId == 101).ShouldBeTrue();
        tours.Results.Any(t => t.Name == "Fruska Gora test ruta").ShouldBeTrue();
    }

    [Fact]
    public void GetByAuthor_returns_empty_when_author_has_no_tours()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, userId: "999999");

        var getResult = (OkObjectResult)controller.GetByAuthor(page: 1, pageSize: 10).Result!;
        var tours = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();

        tours.TotalCount.ShouldBe(0);
        tours.Results.ShouldBeEmpty();
    }

    [Fact]
    public void GetActive_returns_only_published_tours()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var getResult = (OkObjectResult)controller.GetActive(page: 1, pageSize: 10).Result!;
        var tours = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();

        tours.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        tours.Results.All(t => t.Status == "Published").ShouldBeTrue();
        tours.Results.All(t => t.KeyPoints.Count <= 1).ShouldBeTrue();
        tours.Results.Any(t => t.Name == "Dunavska staza" && t.KeyPoints.Count == 1).ShouldBeTrue();
        tours.Results.All(t => t.RouteLengthKm >= 0m).ShouldBeTrue();
        tours.Results.Any(t => t.Name == "Dunavska staza" && t.RouteLengthKm > 0m).ShouldBeTrue();
    }
}
