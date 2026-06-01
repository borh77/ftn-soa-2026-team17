namespace TouristApp.Tours.Core.Domain;

public class CompletedKeyPoint
{
    public long Id { get; private set; }
    public long TourExecutionId { get; private set; }
    public int KeyPointOrdinalNo { get; private set; }
    public DateTime CompletedAt { get; private set; }

    private CompletedKeyPoint() { }

    public CompletedKeyPoint(int keyPointOrdinalNo, DateTime completedAt)
    {
        KeyPointOrdinalNo = keyPointOrdinalNo;
        CompletedAt = completedAt;
    }
}
