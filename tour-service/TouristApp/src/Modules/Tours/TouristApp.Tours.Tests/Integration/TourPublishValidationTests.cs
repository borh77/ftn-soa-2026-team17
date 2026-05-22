using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TouristApp.API.Controllers;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.BuildingBlocks.Core.UseCases;
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

    private static List<TourTravelTimeDto> DefaultTravelTimes() =>
        new() { new(TransportType.Walking, 120) };

    [Fact]
    public void Publish_requires_minimum_two_keypoints()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks;
        var controller = CreateController(scope, userId: authorId.ToString());
        var tourService = GetTourService(scope);

        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string> { "avantura" }, TravelTimes: DefaultTravelTimes());
        var createdResult = (ObjectResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        Should.Throw<EntityValidationException>(() => controller.Publish(created.Id));

        controller.AddKeyPoint(created.Id, new KeyPointDto(null, "P1", "Desc", "S", "i.jpg", 44, 20));
        Should.Throw<EntityValidationException>(() => controller.Publish(created.Id));

        controller.AddKeyPoint(created.Id, new KeyPointDto(null, "P2", "Desc", "S", "j.jpg", 44.1, 20.1));

        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();

        var tour = GetTourFromAuthorPage(tourService, authorId, created.Id);
        tour.Status.ShouldBe("Published");
        tour.PublishedAt.ShouldNotBeNull();
        tour.ArchivedAt.ShouldBeNull();
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
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string> { "avantura" }, TravelTimes: DefaultTravelTimes(), KeyPoints: kps);
        var createdResult = (ObjectResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        controller.Publish(created.Id);

        Should.Throw<EntityValidationException>(() => controller.AddKeyPoint(created.Id, new KeyPointDto(null, "P3", "Desc", "S", "k.jpg", 44.2, 20.2)));
        Should.Throw<EntityValidationException>(() => controller.UpdateKeyPoint(created.Id, 1, new KeyPointDto(1, "Updated", "Desc", "S", "i-updated.jpg", 44.05, 20.05)));
        Should.Throw<EntityValidationException>(() => controller.RemoveKeyPoint(created.Id, 1));
    }

    [Fact]
    public void Create_with_exact_two_keypoints_and_publish_succeeds()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 4;
        var controller = CreateController(scope, userId: authorId.ToString());
        var tourService = GetTourService(scope);

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string> { "avantura" }, TravelTimes: DefaultTravelTimes(), KeyPoints: kps);
        var createdResult = (ObjectResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        created.KeyPoints.Count.ShouldBe(2);
        created.Status.ShouldBe("Draft");

        var publishResult = controller.Publish(created.Id);
        publishResult.ShouldBeOfType<OkResult>();

        var tour = GetTourFromAuthorPage(tourService, authorId, created.Id);
        tour.Status.ShouldBe("Published");
    }

    [Fact]
    public void Publish_requires_at_least_one_tag()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 9;
        var controller = CreateController(scope, userId: authorId.ToString());

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto("Tour", "Desc", "Easy", new List<string>(), TravelTimes: DefaultTravelTimes(), KeyPoints: kps);
        var createdResult = (ObjectResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        Should.Throw<EntityValidationException>(() => controller.Publish(created.Id));
    }

    [Fact]
    public void Archive_and_reactivate_preserve_lifecycle_visibility()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 11;
        var controller = CreateController(scope, userId: authorId.ToString());
        var tourService = GetTourService(scope);

        var kps = new List<KeyPointDto>
        {
            new KeyPointDto(1, "P1", "Desc", "S", "i.jpg", 44, 20),
            new KeyPointDto(2, "P2", "Desc", "S", "j.jpg", 44.1, 20.1)
        };
        var dto = new CreateTourDto(
            "Tour",
            "Desc",
            "Easy",
            new List<string> { "avantura" },
            TravelTimes: DefaultTravelTimes(),
            KeyPoints: kps,
            RouteLengthKm: 14.2m);

        var createdResult = (ObjectResult)controller.Create(dto).Result!;
        var created = createdResult.Value.ShouldBeOfType<TourResponseDto>();

        controller.Publish(created.Id);

        var publishedTour = GetTourFromAuthorPage(tourService, authorId, created.Id);
        publishedTour.Status.ShouldBe("Published");
        publishedTour.PublishedAt.ShouldNotBeNull();
        publishedTour.ArchivedAt.ShouldBeNull();
        publishedTour.RouteLengthKm.ShouldBe(14.2m);

        controller.Archive(created.Id);

        var archivedTour = GetTourFromAuthorPage(tourService, authorId, created.Id);
        archivedTour.Status.ShouldBe("Archived");
        archivedTour.ArchivedAt.ShouldNotBeNull();

        controller.Reactivate(created.Id);

        var reactivatedTour = GetTourFromAuthorPage(tourService, authorId, created.Id);
        reactivatedTour.Status.ShouldBe("Published");
        reactivatedTour.PublishedAt.ShouldNotBeNull();
        reactivatedTour.ArchivedAt.ShouldBeNull();
        reactivatedTour.KeyPoints.Count.ShouldBe(2);
    }

    [Fact]
    public void Publish_requires_defined_transport_type_and_positive_minutes()
    {
        Should.Throw<EntityValidationException>(() =>
            TouristApp.Tours.Core.Domain.Tour.Create(
                1,
                "Tura",
                "Opis",
                TouristApp.Tours.Core.Domain.TourDifficulty.Easy,
                new List<string> { "tag" },
                new List<TouristApp.Tours.Core.Domain.TourTravelTime>
                {
                    new((TouristApp.Tours.Core.Domain.TransportType)99, 10)
                }));

        Should.Throw<EntityValidationException>(() =>
            TouristApp.Tours.Core.Domain.Tour.Create(
                1,
                "Tura",
                "Opis",
                TouristApp.Tours.Core.Domain.TourDifficulty.Easy,
                new List<string> { "tag" },
                new List<TouristApp.Tours.Core.Domain.TourTravelTime>
                {
                    new(TouristApp.Tours.Core.Domain.TransportType.Walking, 0)
                }));
    }

    private static TourResponseDto GetTourFromAuthorPage(ITourService tourService, long authorId, long tourId)
    {
        var paged = tourService.GetByAuthor(authorId, 1, 10);
        return paged.Results.First(r => r.Id == tourId);
    }
}
