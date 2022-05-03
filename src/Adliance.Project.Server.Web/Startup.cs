using System.Globalization;
using Adliance.Project.Server.Core.Extensions;
using Adliance.Project.Server.Infrastructure.Database;
using Adliance.Project.Server.Infrastructure.Extensions;
using Adliance.Project.Server.Web.Extensions;
using Adliance.Project.Server.Web.Middleware;
using Adliance.Project.Server.Web.Services.BackgroundJobs;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Adliance.Project.Server.Web;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddOptions(_configuration, out var dbOptions, out var azureAdOptions, out var backgroundJobsOptions);
        services.AddCoreServices();
        services.AddInfrastructureServices(dbOptions);
        services.AddServices();
        services.AddResponseFactories();
        services.AddAuthenticationAndAuthorization(azureAdOptions);
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllersWithViews(o =>
        {
            // force authorized users by default as a security measure, because then at least a user needs to be authenticated to access anything, even if the programmer misses an [Authorize] attribute
            o.Filters.Add(new AuthorizeFilter());
        });
        services.AddHealthChecks().AddDbContextCheck<Db>();
        services.AddSwagger();
        services.AddBackgroundJobs(dbOptions, backgroundJobsOptions);
    }


    public void Configure(IApplicationBuilder app)
    {
        app.UseHttps();
        app.UseErrorHandling();
        app.UseStaticFiles();
        app.UseRequestLocalization(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("de-DE");
            options.SupportedCultures = options.SupportedUICultures = new List<CultureInfo> { new("de-DE") };
        });
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ApiCallsMiddleware>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapDefaultControllerRoute();
        });
        app.UseSwaggerAndSwaggerUi();
        app.UseBackgroundJobs();
    }
}
