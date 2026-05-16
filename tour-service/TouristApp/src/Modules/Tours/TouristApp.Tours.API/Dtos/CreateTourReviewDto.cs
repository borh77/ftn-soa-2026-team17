namespace TouristApp.Tours.API.Dtos;

public class CreateTourReviewDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime VisitedAt { get; set; }
    public List<string>? Images { get; set; }
}
