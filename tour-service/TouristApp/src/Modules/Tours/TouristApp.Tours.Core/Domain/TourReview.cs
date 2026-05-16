using TouristApp.BuildingBlocks.Core.Domain;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Core.Domain;

public class TourReview : Entity
{
    public long TourId { get; private set; }
    public long TouristId { get; private set; }
    public string TouristUsername { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime VisitedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<string> Images => _images.AsReadOnly();

    private List<string> _images = new();

    private TourReview() { }

    private TourReview(
        long tourId,
        long touristId,
        string touristUsername,
        int rating,
        string comment,
        DateTime visitedAt,
        DateTime createdAt,
        List<string> images)
    {
        TourId = tourId;
        TouristId = touristId;
        TouristUsername = touristUsername;
        Rating = rating;
        Comment = comment;
        VisitedAt = visitedAt;
        CreatedAt = createdAt;
        _images = images;
    }

    public static TourReview Create(
        long tourId,
        long touristId,
        string touristUsername,
        int rating,
        string comment,
        DateTime visitedAt,
        List<string>? images)
    {
        Validate(tourId, touristId, touristUsername, rating, comment, visitedAt, images);

        return new TourReview(
            tourId,
            touristId,
            touristUsername.Trim(),
            rating,
            comment.Trim(),
            visitedAt,
            DateTime.UtcNow,
            images?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).ToList() ?? new List<string>());
    }

    private static void Validate(
        long tourId,
        long touristId,
        string touristUsername,
        int rating,
        string comment,
        DateTime visitedAt,
        List<string>? images)
    {
        if (tourId <= 0)
            throw new EntityValidationException("Tour id must be a positive number.");

        if (touristId <= 0)
            throw new EntityValidationException("Tourist id must be a positive number.");

        if (string.IsNullOrWhiteSpace(touristUsername))
            throw new EntityValidationException("Tourist username is required.");

        if (rating < 1 || rating > 5)
            throw new EntityValidationException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new EntityValidationException("Comment is required.");

        if (comment.Length > 2000)
            throw new EntityValidationException("Comment cannot be longer than 2000 characters.");

        if (visitedAt == default)
            throw new EntityValidationException("Visit date is required.");

        if (visitedAt > DateTime.UtcNow.AddDays(1))
            throw new EntityValidationException("Visit date cannot be in the future.");

        if (images != null && images.Any(i => !string.IsNullOrWhiteSpace(i) && i.Length > 2000000))
            throw new EntityValidationException("Review image is too large.");
    }
}
