using TouristApp.BuildingBlocks.Infrastructure.Database;
using TouristApp.Tours.API.Public;
using TouristApp.Tours.Core.Mappers;
using TouristApp.Tours.Core.UseCases;
using TouristApp.Tours.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace TouristApp.Tours.Infrastructure;

public static class ToursStartup
{
    public static IServiceCollection ConfigureToursModule(this IServiceCollection services)
    {
        // Register AutoMapper profiles from this module's assembly
        services.AddAutoMapper(typeof(ToursProfile).Assembly);

        SetupCore(services);
        SetupInfrastructure(services);

        return services;
    }

    private static void SetupCore(IServiceCollection services)
    {
        services.AddScoped<IHealthService, HealthService>();
    }

    private static void SetupInfrastructure(IServiceCollection services)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            DbConnectionStringBuilder.Build("Tours")); //might change later
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ToursContext>(opt =>
            opt.UseNpgsql(dataSource,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "Tours")));
    }
}
