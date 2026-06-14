using System.Diagnostics;

namespace TouristApp.Tours.Core.Observability;

public static class ToursTelemetry
{
    public const string ActivitySourceName = "TouristApp.Tours";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
