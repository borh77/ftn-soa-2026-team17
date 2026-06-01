using TouristApp.BuildingBlocks.Core.Domain;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Core.Domain;

public class TourExecution : AggregateRoot
{
    public long TourId { get; private set; }
    public long TouristId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? AbandonedAt { get; private set; }
    public DateTime LastActivity { get; private set; }
    public double StartedLatitude { get; private set; }
    public double StartedLongitude { get; private set; }
    public IReadOnlyList<CompletedKeyPoint> CompletedKeyPoints => _completedKeyPoints.AsReadOnly();

    private readonly List<CompletedKeyPoint> _completedKeyPoints = new();

    private TourExecution() { }

    private TourExecution(long tourId, long touristId, double startedLatitude, double startedLongitude, DateTime now)
    {
        TourId = tourId;
        TouristId = touristId;
        StartedLatitude = startedLatitude;
        StartedLongitude = startedLongitude;
        StartedAt = now;
        LastActivity = now;
        Status = TourExecutionStatus.Active;
    }

    public static TourExecution Start(Tour tour, long touristId, double latitude, double longitude)
    {
        if (tour.Status != TourStatus.Published && tour.Status != TourStatus.Archived)
            throw new EntityValidationException("Turista može pokrenuti samo objavljenu ili arhiviranu turu.");

        ValidateTourist(touristId);
        ValidateLocation(latitude, longitude);

        return new TourExecution(tour.Id, touristId, latitude, longitude, DateTime.UtcNow);
    }

    public CompletedKeyPoint? CheckKeyPointProximity(Tour tour, double latitude, double longitude, double thresholdMeters)
    {
        EnsureActive();
        ValidateLocation(latitude, longitude);

        var now = DateTime.UtcNow;
        LastActivity = now;

        var reached = tour.KeyPoints
            .OrderBy(keyPoint => keyPoint.OrdinalNo)
            .FirstOrDefault(keyPoint =>
                !_completedKeyPoints.Any(completed => completed.KeyPointOrdinalNo == keyPoint.OrdinalNo) &&
                DistanceMeters(latitude, longitude, keyPoint.Latitude, keyPoint.Longitude) <= thresholdMeters);

        if (reached == null)
            return null;

        var completedKeyPoint = new CompletedKeyPoint(reached.OrdinalNo, now);
        _completedKeyPoints.Add(completedKeyPoint);

        if (_completedKeyPoints.Count == tour.KeyPoints.Count)
            Complete(now);

        return completedKeyPoint;
    }

    public void Complete()
    {
        EnsureActive();
        Complete(DateTime.UtcNow);
    }

    public void Abandon()
    {
        EnsureActive();
        var now = DateTime.UtcNow;
        Status = TourExecutionStatus.Abandoned;
        AbandonedAt = now;
        LastActivity = now;
    }

    private void Complete(DateTime now)
    {
        Status = TourExecutionStatus.Completed;
        CompletedAt = now;
        LastActivity = now;
    }

    private void EnsureActive()
    {
        if (Status != TourExecutionStatus.Active)
            throw new EntityValidationException("Sesija ture više nije aktivna.");
    }

    private static void ValidateTourist(long touristId)
    {
        if (touristId <= 0)
            throw new EntityValidationException("ID turiste mora biti pozitivan broj.");
    }

    private static void ValidateLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new EntityValidationException("Geografska širina mora biti između -90 i 90 stepeni.");

        if (longitude < -180 || longitude > 180)
            throw new EntityValidationException("Geografska dužina mora biti između -180 i 180 stepeni.");
    }

    private static double DistanceMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double EarthRadiusMeters = 6371000d;

        var lat1 = DegreesToRadians(latitude1);
        var lon1 = DegreesToRadians(longitude1);
        var lat2 = DegreesToRadians(latitude2);
        var lon2 = DegreesToRadians(longitude2);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a = Math.Pow(Math.Sin(deltaLat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;
}
