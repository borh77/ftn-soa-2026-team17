using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TouristApp.BuildingBlocks.Core.UseCases;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.API.Controllers;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Tests.Integration;

[Collection("Sequential")]
public class TourKeyPointCommandTests : BaseToursIntegrationTest
{
    public TourKeyPointCommandTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void AddKeyPoint_endpoint_adds_keypoint()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var authorId = DateTime.UtcNow.Ticks;
        var dto = new CreateTourDto(
            Name: "City walk",
            Description: "Opis",
            Difficulty: "Easy",
            Tags: new List<string>()
        );

        var createdResult = (CreatedAtActionResult)controller.Create(authorId, dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        var addResult = controller.AddKeyPoint(created.Id, kpDto);
        addResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(authorId, 1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(1);
        tour.KeyPoints[0].Name.ShouldBe("Museum");
    }

    [Fact]
    public void UpdateKeyPoint_endpoint_updates_keypoint()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var authorId = DateTime.UtcNow.Ticks + 1;
        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(authorId, dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        controller.AddKeyPoint(created.Id, kpDto);

        var updateDto = new KeyPointDto(1, "Updated", "Updated desc", "Secret", "updated.jpg", 45.0, 21.0);
        var updateResult = controller.UpdateKeyPoint(created.Id, 1, updateDto);
        updateResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(authorId, 1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints[0].Name.ShouldBe("Updated");
        tour.KeyPoints[0].Latitude.ShouldBe(45.0);
    }

    [Fact]
    public void RemoveKeyPoint_endpoint_removes_keypoint()
    {
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope);

        var authorId = DateTime.UtcNow.Ticks + 2;
        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(authorId, dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        controller.AddKeyPoint(created.Id, kpDto);

        var removeResult = controller.RemoveKeyPoint(created.Id, 1);
        removeResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(authorId, 1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(0);
    }

    private static ToursController CreateController(IServiceScope scope) =>
        new(
            scope.ServiceProvider.GetRequiredService<IHealthService>(),
            scope.ServiceProvider.GetRequiredService<ITourService>())
        {
            ControllerContext = BuildContext("-1")
        };
}
