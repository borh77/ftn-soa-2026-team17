using TouristApp.Tours.Infrastructure;

namespace TouristApp.API.Startup;

public static class ModulesConfiguration
{
    public static IServiceCollection RegisterModules(this IServiceCollection services)
    {
        services.ConfigureToursModule();

        return services;
    }
}
