using Shouldly;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.Core.Domain;
using Xunit;

namespace TouristApp.Tours.Tests.Unit;

public class TourStatusTests
{
    private static List<TourTravelTime> DefaultTravelTimes() =>
        new() { new(TransportType.Walking, 120) };

    private static List<string> DefaultTags() =>
        new() { "planina" };

    [Fact]
    public void Publish_requires_at_least_two_keypoints()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        Should.Throw<EntityValidationException>(() => tour.Publish());
    }

    [Fact]
    public void Publish_succeeds_with_two_keypoints()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        var before = DateTime.UtcNow;
        tour.Publish();
        var after = DateTime.UtcNow;

        tour.Status.ShouldBe(TourStatus.Published);
        tour.RouteLengthKm.ShouldBeGreaterThan(0m);
        tour.PublishedAt.ShouldNotBeNull();
        (tour.PublishedAt!.Value >= before).ShouldBeTrue();
        (tour.PublishedAt.Value <= after).ShouldBeTrue();
        tour.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Route_length_is_zero_until_second_keypoint_is_added()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());

        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        tour.RouteLengthKm.ShouldBe(0m);

        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.RouteLengthKm.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public void Archive_sets_status_to_archived()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();
        var before = DateTime.UtcNow;
        tour.Archive();
        var after = DateTime.UtcNow;

        tour.Status.ShouldBe(TourStatus.Archived);
        tour.ArchivedAt.ShouldNotBeNull();
        (tour.ArchivedAt!.Value >= before).ShouldBeTrue();
        (tour.ArchivedAt.Value <= after).ShouldBeTrue();
    }

    [Fact]
    public void Archive_requires_published_tour()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());

        Should.Throw<EntityValidationException>(() => tour.Archive());
    }

    [Fact]
    public void Reactivate_sets_status_back_to_published()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();
        tour.Archive();
        var before = DateTime.UtcNow;
        tour.Reactivate();
        var after = DateTime.UtcNow;

        tour.Status.ShouldBe(TourStatus.Published);
        tour.ArchivedAt.ShouldBeNull();
        tour.PublishedAt.ShouldNotBeNull();
        (tour.PublishedAt!.Value >= before).ShouldBeTrue();
        (tour.PublishedAt.Value <= after).ShouldBeTrue();
    }

    [Fact]
    public void Reactivate_requires_archived_tour()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());

        Should.Throw<EntityValidationException>(() => tour.Reactivate());
    }

    [Fact]
    public void Archive_then_publish_returns_to_published_if_keypoints_exist()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();
        var archivedBefore = DateTime.UtcNow;
        tour.Archive();
        tour.ArchivedAt.ShouldNotBeNull();
        tour.Publish();

        tour.Status.ShouldBe(TourStatus.Published);
        tour.PublishedAt.ShouldNotBeNull();
        tour.ArchivedAt.ShouldBeNull();
        (tour.PublishedAt!.Value >= archivedBefore).ShouldBeTrue();
    }

    [Fact]
    public void Modifications_not_allowed_when_published()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, DefaultTags(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();

        Should.Throw<EntityValidationException>(() => tour.AddKeyPoint(new KeyPoint(3, "C", "c", "s", "k.jpg", 44.2, 20.2)));
        Should.Throw<EntityValidationException>(() => tour.RemoveKeyPoint(1));
        Should.Throw<EntityValidationException>(() => tour.UpdateKeyPoint(1, new KeyPointUpdate("x","x","x","x",44,20)));
    }

    [Fact]
    public void Modifications_allowed_when_draft()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        // still draft
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.RemoveKeyPoint(1);
        tour.UpdateKeyPoint(1, new KeyPointUpdate("B","bb","s","j.jpg",44.1,20.1));

        tour.KeyPoints.Count.ShouldBe(1);
    }

    [Fact]
    public void Publish_requires_at_least_one_tag()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        Should.Throw<EntityValidationException>(() => tour.Publish());
    }
}
