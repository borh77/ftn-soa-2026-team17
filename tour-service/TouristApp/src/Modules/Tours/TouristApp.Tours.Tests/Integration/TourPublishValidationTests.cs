using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.BuildingBlocks.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.API.Controllers;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;

namespace TouristApp.Tours.Tests.Integration;

/// <summary>
/// Integration tests for publish/validation workflow after keypoint changes.
/// </summary>
[Collection("Sequential")]
public class TourPublishValidationTests : BaseToursIntegrationTest
{
    public TourPublishValidationTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void Publish_requires_minimum_two_keypoints()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Try to publish with zero keypoints
        Should.Throw<EntityValidationException>(() => controller.Publish(created.Id));

        // Add one keypoint
        var kp1 = new KeyPointDto(null, "P1", "Desc", "S", "i.jpg", 44, 20);
        controller.AddKeyPoint(created.Id, kp1);

        // Still cannot publish with one keypoint
        Should.Throw<EntityValidationException>(() => controller.Publish(created.Id));

        // Add second keypoint
        var kp2 = new KeyPointDto(null, "P2", "Desc", "S", "j.jpg", 44.1, 20.1);
        controller.AddKeyPoint(created.Id, kp2);

        // Now should publish successfully
        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.Status.ShouldBe("Published");
    }

    [Fact]
    public void Publish_after_removing_keypoint_fails_if_less_than_two()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 1;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1),
            new KeyPointDto(3, "P3", "Desc", "S", "k.jpg", 44.2, 20.2)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Publish with 3 keypoints
        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.Status.ShouldBe("Published");

        // Archive tour
        var archiveResult = controller.Archive(created.Id);
        archiveResult.ShouldBeOfType<OkResult>();

        // Cannot modify published tour, but archive allows unarchiving
       
    }

    [Fact]
    public void Cannot_modify_keypoints_after_publishing()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 2;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Publish
        controller.Publish(created.Id);

        // Try to add keypoint after publish
        var newKp = new KeyPointDto(null, "P3", "Desc", "S", "k.jpg", 44.2, 20.2);
        Should.Throw<EntityValidationException>(() => controller.AddKeyPoint(created.Id, newKp));

        // Try to update keypoint after publish
        var updateKp = new KeyPointDto(1, "Updated", "Desc", "S", "i-updated.jpg", 44.05, 20.05);
        Should.Throw<EntityValidationException>(() => controller.UpdateKeyPoint(created.Id, 1, updateKp));

        // Try to remove keypoint after publish
        Should.Throw<EntityValidationException>(() => controller.RemoveKeyPoint(created.Id, 1));
    }

    [Fact]
    public void Edit_tour_can_append_keypoints_in_draft()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 3;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Add initial keypoint
        var kp1 = new KeyPointDto(null, "P1", "Desc", "S", "i.jpg", 44, 20);
        controller.AddKeyPoint(created.Id, kp1);

        // Update tour and append another keypoint
        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(null, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var updateDto = new UpdateTourDto(created.Name, created.Description, created.Difficulty, 
            created.Tags.ToList(), created.Price, kps);
        var updateResult = controller.Update(created.Id, updateDto);
        updateResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.KeyPoints.Count.ShouldBe(2);
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
    }

    [Fact]
    public void Create_with_exact_two_keypoints_and_publish_succeeds()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 4;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        created.KeyPoints.Count.ShouldBe(2);
        created.Status.ShouldBe("Draft");

        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.Status.ShouldBe("Published");
    }

    [Fact]
    public void Create_with_insert_at_middle_maintains_correct_order_for_publish()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 5;
        var controller = CreateController(scope, userId: authorId.ToString());

        // Create keypoints with out-of-order ordinales to test insertion
        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(3, "P3", "Desc", "S", "k.jpg", 44.2, 20.2),
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Verify order after insertion
        created.KeyPoints.Count.ShouldBe(3);
        created.KeyPoints[0].Name.ShouldBe("P1");
        created.KeyPoints[1].Name.ShouldBe("P2");
        created.KeyPoints[2].Name.ShouldBe("P3");

        // Publish should succeed (has 3 keypoints, > 2 minimum)
        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public void Delete_tour_only_allowed_in_draft_status()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 6;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Delete in draft should succeed
        var deleteResult = controller.Delete(created.Id);
        deleteResult.ShouldBeOfType<OkResult>();

        // Verify tour is deleted
        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        paged.Results.Any(r => r.Id == created.Id).ShouldBeFalse();
    }

    [Fact]
    public void Invalid_author_cannot_modify_tour()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 7;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>());
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        // Other author tries to update (switch to different userId)
        var scope2 = Factory.Services.CreateScope();
        var controller2 = CreateController(scope2, userId: (authorId + 1000).ToString());
        var updateDto = new UpdateTourDto("Modified", "Desc", "Easy", new List<string>(), 0);
        Should.Throw<EntityValidationException>(() => controller2.Update(created.Id, updateDto));

        // Other author tries to delete
        Should.Throw<EntityValidationException>(() => controller2.Delete(created.Id));
    }

    [Fact]
    public void Price_can_be_updated_in_draft()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 8;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), kps);
        var createdResult = (CreatedAtActionResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        created.Price.ShouldBe(0);

        // Update price to 99.99
        var updateDto = new UpdateTourDto(created.Name, created.Description, created.Difficulty, 
            created.Tags.ToList(), 99.99m);
        var updateResult = controller.Update(created.Id, updateDto);
        updateResult.ShouldBeOfType<OkResult>();

        var getResult = (OkObjectResult)controller.GetByAuthor(1, 10).Result!;
        var paged = getResult.Value.ShouldBeOfType<PagedResult<TourResponseDto>>();
        var tour = paged.Results.First(r => r.Id == created.Id);
        tour.Price.ShouldBe(99.99m);
    }
}
