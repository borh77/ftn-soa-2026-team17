using TouristApp.BuildingBlocks.Tests;
using TouristApp.Tours.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.BuildingBlocks.Tests;

namespace TouristApp.Tours.Tests;

public class ToursTestFactory : BaseTestFactory<ToursContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ToursContext>));
        if (descriptor != null) services.Remove(descriptor);

        services.AddDbContext<ToursContext>(SetupTestContext());

        return services;
    }
}
