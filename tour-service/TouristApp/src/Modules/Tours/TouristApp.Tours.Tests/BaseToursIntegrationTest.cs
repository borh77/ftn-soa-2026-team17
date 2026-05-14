using TouristApp.BuildingBlocks.Tests;
using TouristApp.Tours.Infrastructure.Database;

namespace TouristApp.Tours.Tests;

/// <summary>
/// Base class for Tours integration tests.
/// </summary>
public abstract class BaseToursIntegrationTest : BaseWebIntegrationTest<ToursTestFactory>
{
    protected BaseToursIntegrationTest(ToursTestFactory factory) : base(factory) { }
}
