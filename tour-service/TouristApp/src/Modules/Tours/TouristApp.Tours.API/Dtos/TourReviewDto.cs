namespace TouristApp.Tours.API.Dtos;

public class TourReviewDto
{
    public long Id { get; set; }
    public long TourId { get; set; }
    public long TouristId { get; set; }
    public string TouristUsername { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime VisitedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Images { get; set; } = new();
}
