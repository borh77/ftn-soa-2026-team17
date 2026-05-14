using Shouldly;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.Core.Domain;
using Xunit;

namespace TouristApp.Tours.Tests.Unit;

public class TourStatusTests
{
    [Fact]
    public void Publish_requires_at_least_two_keypoints()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        Should.Throw<EntityValidationException>(() => tour.Publish());
    }

    [Fact]
    public void Publish_succeeds_with_two_keypoints()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();

        tour.Status.ShouldBe(TourStatus.Published);
    }

    [Fact]
    public void Archive_sets_status_to_archived()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();
        tour.Archive();

        tour.Status.ShouldBe(TourStatus.Archived);
    }

    [Fact]
    public void Archive_then_publish_returns_to_published_if_keypoints_exist()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.Publish();
        tour.Archive();
        tour.Publish();

        tour.Status.ShouldBe(TourStatus.Published);
    }

    [Fact]
    public void Modifications_not_allowed_when_published()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
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
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        // still draft
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.RemoveKeyPoint(1);
        tour.UpdateKeyPoint(1, new KeyPointUpdate("B","bb","s","j.jpg",44.1,20.1));

        tour.KeyPoints.Count.ShouldBe(1);
    }
}
