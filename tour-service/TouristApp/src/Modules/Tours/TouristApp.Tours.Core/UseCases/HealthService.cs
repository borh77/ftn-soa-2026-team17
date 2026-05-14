using TouristApp.Tours.API.Public;

namespace TouristApp.Tours.Core.UseCases;

/// <summary>
/// Stub implementation of IHealthService
/// </summary>
public class HealthService : IHealthService
{
    public string Ping() => "Tours module is alive.";
}
