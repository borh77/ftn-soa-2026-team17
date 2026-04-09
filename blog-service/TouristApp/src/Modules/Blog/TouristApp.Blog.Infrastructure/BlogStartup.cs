using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TouristApp.Blog.API.Public;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.Blog.Core.Mappers;
using TouristApp.Blog.Core.UseCases;
using TouristApp.Blog.Infrastructure.Database;
using TouristApp.Blog.Infrastructure.Database.Repositories;
using TouristApp.BuildingBlocks.Infrastructure.Database;

namespace TouristApp.Blog.Infrastructure;

public static class BlogStartup
{
    public static IServiceCollection ConfigureBlogModule(this IServiceCollection services)
    {
        // Register AutoMapper profiles from this module's assembly
        services.AddAutoMapper(typeof(BlogProfile).Assembly);

        SetupCore(services);
        SetupInfrastructure(services);

        return services;
    }

    private static void SetupCore(IServiceCollection services)
    {
        services.AddScoped<IBlogEntryService, BlogEntryService>();
    }

    private static void SetupInfrastructure(IServiceCollection services)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            DbConnectionStringBuilder.Build("blog")); //might change later
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddScoped<IBlogEntryRepository, BlogEntryDbRepository>();

        services.AddDbContext<BlogContext>(opt =>
            opt.UseNpgsql(dataSource,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "blog")));
    }
}
