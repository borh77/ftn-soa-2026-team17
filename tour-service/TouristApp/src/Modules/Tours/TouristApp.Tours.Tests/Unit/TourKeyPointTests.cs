using Shouldly;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.Core.Domain;
using Xunit;

namespace TouristApp.Tours.Tests.Unit;

public class TourKeyPointTests
{
    [Fact]
    public void AddKeyPoint_succeeds()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var keyPoint = new KeyPoint(1, "Museum", "A famous museum", "Secret entrance", "museum.jpg", 44.82, 20.45);

        // Act
        tour.AddKeyPoint(keyPoint);

        // Assert
        tour.KeyPoints.Count.ShouldBe(1);
        tour.KeyPoints[0].Name.ShouldBe("Museum");
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
    }

    [Fact]
    public void AddKeyPoint_at_same_ordinal_shifts_existing()
    {
        // Arrange: test that inserting at same ordinal shifts existing keypoints
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var kp1 = new KeyPoint(1, "Museum", "A famous museum", "Secret", "museum.jpg", 44.82, 20.45);
        var kp2 = new KeyPoint(1, "Park", "A public park", "Secret", "park.jpg", 44.83, 20.46);

        tour.AddKeyPoint(kp1);

        // Act: insert at ordinal 1 (should shift kp1 to ordinal 2)
        tour.AddKeyPoint(kp2);

        // Assert
        tour.KeyPoints.Count.ShouldBe(2);
        tour.KeyPoints[0].Name.ShouldBe("Park");
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].Name.ShouldBe("Museum");
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
    }

    [Fact]
    public void AddKeyPoint_throws_when_null()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => tour.AddKeyPoint(null!));
    }

    [Fact]
    public void RemoveKeyPoint_succeeds()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var keyPoint = new KeyPoint(1, "Museum", "A famous museum", "Secret", "museum.jpg", 44.82, 20.45);
        tour.AddKeyPoint(keyPoint);

        // Act
        tour.RemoveKeyPoint(1);

        // Assert
        tour.KeyPoints.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveKeyPoint_does_nothing_when_ordinal_not_found()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var keyPoint = new KeyPoint(1, "Museum", "A famous museum", "Secret", "museum.jpg", 44.82, 20.45);
        tour.AddKeyPoint(keyPoint);

        // Act
        tour.RemoveKeyPoint(99);

        // Assert
        tour.KeyPoints.Count.ShouldBe(1);
    }

    [Fact]
    public void UpdateKeyPoint_succeeds()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var keyPoint = new KeyPoint(1, "Museum", "A famous museum", "Secret", "museum.jpg", 44.82, 20.45);
        tour.AddKeyPoint(keyPoint);

        var update = new KeyPointUpdate(
            "Updated Museum",
            "Updated description",
            "Updated secret",
            "updated.jpg",
            45.0,
            21.0
        );

        // Act
        tour.UpdateKeyPoint(1, update);

        // Assert
        tour.KeyPoints[0].Name.ShouldBe("Updated Museum");
        tour.KeyPoints[0].Description.ShouldBe("Updated description");
        tour.KeyPoints[0].Latitude.ShouldBe(45.0);
        tour.KeyPoints[0].Longitude.ShouldBe(21.0);
    }

    [Fact]
    public void UpdateKeyPoint_throws_when_ordinal_not_found()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var update = new KeyPointUpdate("Updated", "Updated", "Updated", "updated.jpg", 45.0, 21.0);

        // Act & Assert
        Should.Throw<EntityValidationException>(() => tour.UpdateKeyPoint(1, update));
    }

    [Fact]
    public void UpdateKeyPoint_throws_when_update_null()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var keyPoint = new KeyPoint(1, "Museum", "A famous museum", "Secret", "museum.jpg", 44.82, 20.45);
        tour.AddKeyPoint(keyPoint);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => tour.UpdateKeyPoint(1, null!));
    }

    [Fact]
    public void ClearKeyPoints_succeeds()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "Museum", "A museum", "Secret", "museum.jpg", 44.82, 20.45));
        tour.AddKeyPoint(new KeyPoint(2, "Park", "A park", "Secret", "park.jpg", 44.83, 20.46));

        // Act
        tour.ClearKeyPoints();

        // Assert
        tour.KeyPoints.Count.ShouldBe(0);
    }

    [Fact]
    public void KeyPoint_validation_fails_with_invalid_latitude()
    {
        // Act & Assert
        Should.Throw<EntityValidationException>(
            () => new KeyPoint(1, "Museum", "A museum", "Secret", "museum.jpg", 91, 20)
        );
    }

    [Fact]
    public void KeyPoint_validation_fails_with_invalid_longitude()
    {
        // Act & Assert
        Should.Throw<EntityValidationException>(
            () => new KeyPoint(1, "Museum", "A museum", "Secret", "museum.jpg", 44, 181)
        );
    }

    [Fact]
    public void KeyPoint_validation_fails_with_invalid_ordinal()
    {
        // Act & Assert
        Should.Throw<EntityValidationException>(
            () => new KeyPoint(0, "Museum", "A museum", "Secret", "museum.jpg", 44, 20)
        );
    }

    [Fact]
    public void AddMultipleKeyPoints_recalculates_ordinals()
    {
        // Arrange
        var tour = Tour.Create(1, "Test Tour", "Test Description", TourDifficulty.Easy, new List<string>());
        var kp1 = new KeyPoint(1, "Museum", "A museum", "Secret", "museum.jpg", 44, 20);
        var kp2 = new KeyPoint(2, "Park", "A park", "Secret", "park.jpg", 45, 21);
        var kp3 = new KeyPoint(3, "Monument", "A monument", "Secret", "monument.jpg", 46, 22);

        // Act
        tour.AddKeyPoint(kp1);
        tour.AddKeyPoint(kp2);
        tour.AddKeyPoint(kp3);

        // Assert
        tour.KeyPoints.Count.ShouldBe(3);
        tour.KeyPoints[0].OrdinalNo.ShouldBe(1);
        tour.KeyPoints[1].OrdinalNo.ShouldBe(2);
        tour.KeyPoints[2].OrdinalNo.ShouldBe(3);
    }
}
