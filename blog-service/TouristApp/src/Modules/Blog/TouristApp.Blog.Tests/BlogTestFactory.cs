using TouristApp.BuildingBlocks.Tests;
using TouristApp.Blog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.BuildingBlocks.Tests;

namespace TouristApp.Blog.Tests;

public class BlogTestFactory : BaseTestFactory<BlogContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<BlogContext>));
        if (descriptor != null) services.Remove(descriptor);

        services.AddDbContext<BlogContext>(SetupTestContext());

        return services;
    }
}
