using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TouristApp.Tours.Infrastructure.Database;
using TouristApp.BuildingBlocks.Tests;
using TouristApp.BuildingBlocks.Infrastructure.Database; 

namespace TouristApp.Tours.Tests;

public class ToursTestFactory : BaseTestFactory<ToursContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ToursContext>));
        if (descriptor != null) services.Remove(descriptor);

        var connectionString = DbConnectionStringBuilder.Build("Tours");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson(); // jsonb!
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ToursContext>(options =>
            options.UseNpgsql(dataSource, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory", "Tours")));

        return services;
    }
}