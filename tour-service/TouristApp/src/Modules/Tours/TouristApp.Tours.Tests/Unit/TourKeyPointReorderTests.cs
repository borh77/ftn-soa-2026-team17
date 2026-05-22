using Shouldly;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.Core.Domain;
using Xunit;

namespace TouristApp.Tours.Tests.Unit;

/// <summary>
/// Unit tests for keypoint reorder/insert-at-position behavior.
/// </summary>
public class TourKeyPointReorderTests
{
    private static List<TourTravelTime> DefaultTravelTimes() =>
        new() { new(TransportType.Walking, 120) };

    [Fact]
    public void AddKeyPoint_at_beginning_shifts_existing_ordinales_up()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        // Insert at position 1, should shift A→2, B→3
        tour.AddKeyPoint(new KeyPoint(1, "New", "n", "s", "k.jpg", 44.2, 20.2));

        tour.KeyPoints.Count.ShouldBe(3);
        tour.KeyPoints[0].Name.ShouldBe("New");
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].Name.ShouldBe("A");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[2].Name.ShouldBe("B");
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
    }

    [Fact]
    public void AddKeyPoint_in_middle_shifts_affected_ordinales()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.AddKeyPoint(new KeyPoint(3, "C", "c", "s", "k.jpg", 44.2, 20.2));

        // Insert at position 2, should shift B→3, C→4
        tour.AddKeyPoint(new KeyPoint(2, "Middle", "m", "s", "l.jpg", 44.3, 20.3));

        tour.KeyPoints.Count.ShouldBe(4);
        tour.KeyPoints[0].Name.ShouldBe("A");
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].Name.ShouldBe("Middle");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[2].Name.ShouldBe("B");
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
        tour.KeyPoints[3].Name.ShouldBe("C");
        tour.KeyPoints[3].OrdinalNo.ShouldBe(4);
    }

    [Fact]
    public void AddKeyPoint_at_end_appends_without_shifting()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        // Append at end (ordinal 3 when count=2)
        tour.AddKeyPoint(new KeyPoint(3, "C", "c", "s", "k.jpg", 44.2, 20.2));

        tour.KeyPoints.Count.ShouldBe(3);
        tour.KeyPoints[2].Name.ShouldBe("C");
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
    }

    [Fact]
    public void AddKeyPoint_with_ordinal_greater_than_count_plus_one_throws()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));

        // Attempt to insert at ordinal 5 when count=1 (max=2)
        Should.Throw<EntityValidationException>(() => 
            tour.AddKeyPoint(new KeyPoint(5, "Far", "f", "s", "z.jpg", 44.5, 20.5)));
    }

    [Fact]
    public void RemoveKeyPoint_then_add_recalculates_ordinales()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.AddKeyPoint(new KeyPoint(3, "C", "c", "s", "k.jpg", 44.2, 20.2));

        tour.RemoveKeyPoint(2); // Remove B

        tour.KeyPoints.Count.ShouldBe(2);
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);

        // Add at position 2 (after A)
        tour.AddKeyPoint(new KeyPoint(2, "NewB", "nb", "s", "nb.jpg", 44.15, 20.15));

        tour.KeyPoints.Count.ShouldBe(3);
        tour.KeyPoints[1].Name.ShouldBe("NewB");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[2].Name.ShouldBe("C");
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
    }

    [Fact]
    public void UpdateKeyPoint_does_not_affect_ordinales_of_others()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.AddKeyPoint(new KeyPoint(3, "C", "c", "s", "k.jpg", 44.2, 20.2));

        tour.UpdateKeyPoint(2, new KeyPointUpdate("B-Updated", "b-desc", "s-new", "j-new.jpg", 45.1, 21.1));

        tour.KeyPoints.Count.ShouldBe(3);
        tour.KeyPoints[1].Name.ShouldBe("B-Updated");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
    }

    [Fact]
    public void Multiple_inserts_at_different_positions_maintain_correct_order()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);

        tour.AddKeyPoint(new KeyPoint(1, "New1", "n1", "s", "n1.jpg", 44.1, 20.1)); // Insert at start
        tour.KeyPoints[0].Name.ShouldBe("New1");
        tour.KeyPoints[1].Name.ShouldBe("A");

        tour.AddKeyPoint(new KeyPoint(3, "New2", "n2", "s", "n2.jpg", 44.2, 20.2)); // Insert at end
        tour.KeyPoints[2].Name.ShouldBe("New2");

        tour.AddKeyPoint(new KeyPoint(2, "Middle", "m", "s", "m.jpg", 44.3, 20.3)); // Insert in middle
        tour.KeyPoints[1].Name.ShouldBe("Middle");
        tour.KeyPoints[2].Name.ShouldBe("A");
        tour.KeyPoints[3].Name.ShouldBe("New2");

        // Verify all ordinales are sequential
        for (int i = 0; i < tour.KeyPoints.Count; i++)
        {
            tour.KeyPoints[i].OrdinalNo.ShouldBe(i + 1);
        }
    }

    [Fact]
    public void ClearKeyPoints_removes_all()
    {
        var tour = Tour.Create(1, "Tour", "Desc", TourDifficulty.Easy, new List<string>(), DefaultTravelTimes());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));

        tour.ClearKeyPoints();

        tour.KeyPoints.Count.ShouldBe(0);
    }
}
