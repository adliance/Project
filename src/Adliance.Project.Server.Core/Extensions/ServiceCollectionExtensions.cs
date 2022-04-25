using Adliance.Project.Server.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Adliance.Project.Server.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddTransient<ArticleSearchService>();
        return services;
    }
}