namespace TouristApp.Tours.API.Dtos;

public class TourPurchaseInfoDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanBePurchased { get; set; }
}