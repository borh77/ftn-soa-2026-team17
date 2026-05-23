using Microsoft.Extensions.DependencyInjection;
using TouristApp.API.Controllers;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.BuildingBlocks.Tests;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Infrastructure.Database;

namespace TouristApp.Tours.Tests;

/// <summary>
/// Base class for Tours integration tests.
/// </summary>
public abstract class BaseToursIntegrationTest : BaseWebIntegrationTest<ToursTestFactory>
{
    protected BaseToursIntegrationTest(ToursTestFactory factory) : base(factory) { }

    /// <summary>
    /// Kreira ToursController sa postavljenim ControllerContext (userId iz claimsa).
    /// </summary>
    protected static ToursController CreateController(IServiceScope scope, string userId = "-1") =>
        new(
            scope.ServiceProvider.GetRequiredService<IHealthService>(),
            scope.ServiceProvider.GetRequiredService<ITourService>(),
            scope.ServiceProvider.GetRequiredService<ITourReviewService>())
        {
            ControllerContext = BuildContext(userId)
        };

    protected static ITourService GetTourService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ITourService>();
}
