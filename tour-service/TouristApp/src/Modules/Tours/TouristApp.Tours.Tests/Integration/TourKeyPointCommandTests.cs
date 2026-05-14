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
        var authorId = DateTime.UtcNow.Ticks;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto(
            Name: "City walk",
            Description: "Opis",
            Difficulty: "Easy",
            Tags: new List<string>()
        );

        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        var addResult = controller.AddKeyPoint(created.Id, kpDto);
        addResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(1);
        tour.KeyPoints[0].Name.ShouldBe("Museum");
    }

    [Fact]
    public void UpdateKeyPoint_endpoint_updates_keypoint()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 1;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        controller.AddKeyPoint(created.Id, kpDto);

        var updateDto = new KeyPointDto(1, "Updated", "Updated desc", "Secret", "updated.jpg", 45.0, 21.0);
        var updateResult = controller.UpdateKeyPoint(created.Id, 1, updateDto);
        updateResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints[0].Name.ShouldBe("Updated");
        tour.KeyPoints[0].Latitude.ShouldBe(45.0);
    }

    [Fact]
    public void RemoveKeyPoint_endpoint_removes_keypoint()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 2;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var kpDto = new KeyPointDto(1, "Museum", "Desc", "Secret", "img.jpg", 44.82, 20.45);
        controller.AddKeyPoint(created.Id, kpDto);

        var removeResult = controller.RemoveKeyPoint(created.Id, 1);
        removeResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(0);
    }

    [Fact]
    public void CreateTour_with_initial_keypoints_persists_and_orders()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 3;
        var controller = CreateController(scope, userId: authorId.ToString());

        // Provide two keypoints with ordinals out of order to test insertion and ordering
        var initialKps = new List<KeyPointDto>
        {
            new KeyPointDto(2, "Second", "Desc2", "S", "img2.jpg", 44.9, 20.5),
            new KeyPointDto(1, "First", "Desc1", "S", "img1.jpg", 44.8, 20.4)
        };

        var dto = new CreateTourDto("CityOrder", "Desc", "Easy", new List<string>(), initialKps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(2);
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[0].Name.ShouldBe("First");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[1].Name.ShouldBe("Second");
    }

    [Fact]
    public void CreateTour_with_invalid_large_ordinal_throws()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 4;
        var controller = CreateController(scope, userId: authorId.ToString());

        var initialKps = new List<KeyPointDto>
        {
            new KeyPointDto(100, "Far", "Desc", "S", "img.jpg", 44.8, 20.4)
        };

        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>(), initialKps);
        Should.Throw<EntityValidationException>(() => ((CreatedAtActionResult)controller.Create(dto).Result!).Value);
    }

    [Fact]
    public void CreateTour_with_negative_ordinal_throws()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 5;
        var controller = CreateController(scope, userId: authorId.ToString());

        var initialKps = new List<KeyPointDto>
        {
            new KeyPointDto(-1, "Neg", "Desc", "S", "img.jpg", 44.8, 20.4)
        };

        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>(), initialKps);
        Should.Throw<EntityValidationException>(() => ((CreatedAtActionResult)controller.Create(dto).Result!).Value);
    }

    [Fact]
    public void CreateTour_with_duplicate_ordinals_shifts_existing()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 6;
        var controller = CreateController(scope, userId: authorId.ToString());

        var initialKps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "A", "Desc", "S", "img.jpg", 44.8, 20.4),
            new KeyPointDto(1, "B", "Desc", "S", "img2.jpg", 44.9, 20.5)
        };

        var dto = new CreateTourDto("City", "Desc", "Easy", new List<string>(), initialKps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Duplicate ordinals cause shifting: first KeyPointDto(1, "A") is added, then KeyPointDto(1, "B") shifts A to 2
        created.KeyPoints.Count.ShouldBe(2);
        created.KeyPoints[0].Name.ShouldBe("B");
        created.KeyPoints[0].OrdinalNo.ShouldBe(1);
        created.KeyPoints[1].Name.ShouldBe("A");
        created.KeyPoints[1].OrdinalNo.ShouldBe(2);
    }

    [Fact]
    public void UpdateTour_can_append_keypoints()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 7;
        var controller = CreateController(scope, userId: authorId.ToString());

        var createDto = new CreateTourDto("City", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(createDto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        var updateKps = new List<KeyPointDto>
        {
            new KeyPointDto(null, "Appended", "Desc", "S", "img.jpg", 44.8, 20.4)
        };

        var updateDto = new UpdateTourDto(created.Name, created.Description, created.Difficulty, created.Tags.ToList(), created.Price, updateKps);
        var updateResult = controller.Update(created.Id, updateDto);
        updateResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<BuildingBlocks.Core.UseCases.PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(1);
        tour.KeyPoints[0].Name.ShouldBe("Appended");
    }
}
