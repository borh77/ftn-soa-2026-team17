using Microsoft.OpenApi.Models;

namespace TouristApp.API.Startup;

public static class SwaggerConfiguration
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TouristApp",
                Version = "v1"
            });
        });
        return services;
    }
}
