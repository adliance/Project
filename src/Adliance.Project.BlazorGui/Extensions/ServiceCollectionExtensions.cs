using Adliance.Project.BlazorGui.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Adliance.Project.BlazorGui.Services;

namespace Adliance.Project.BlazorGui.Extensions;

/// <summary>
/// Provides extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddAuthorizationCore();
        services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
        services.TryAddSingleton(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
        return services;
    }

    /// <summary>
    /// Adds all services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ArticleService>();
        return services;
    }
}
