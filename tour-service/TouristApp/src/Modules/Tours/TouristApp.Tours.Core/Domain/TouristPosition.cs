namespace TouristApp.Tours.Core.Domain;

public class TouristPosition
{
    public long Id { get; private set; }
    public long TouristId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TouristPosition() { }

    public TouristPosition(long touristId, double latitude, double longitude)
    {
        TouristId = touristId;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }
}