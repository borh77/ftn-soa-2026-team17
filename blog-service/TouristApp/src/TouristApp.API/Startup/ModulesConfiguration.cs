using TouristApp.Blog.Infrastructure;

namespace TouristApp.API.Startup;

public static class ModulesConfiguration
{
    public static IServiceCollection RegisterModules(this IServiceCollection services)
    {
        services.ConfigureBlogModule();

        return services;
    }
}
