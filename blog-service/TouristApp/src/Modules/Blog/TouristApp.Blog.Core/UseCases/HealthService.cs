using TouristApp.Blog.API.Public;

namespace TouristApp.Blog.Core.UseCases;

/// <summary>
/// Stub implementation of IHealthService
/// </summary>
public class HealthService : IHealthService
{
    public string Ping() => "Blog module is alive.";
}
