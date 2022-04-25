using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Infrastructure.Database;
using Adliance.Project.Server.Web.Options;
using Adliance.Project.Server.Web.Services.Authentication;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Adliance.Project.Server.Web.Services.BackgroundJobs;

public class BackgroundJobs
{
    private readonly IServiceProvider _services;
    private readonly BackgroundJobsOptions _backgroundJobsOptions;

    public BackgroundJobs(IServiceProvider services, IOptions<BackgroundJobsOptions> backgroundJobsOptions)
    {
        _services = services;
        _backgroundJobsOptions = backgroundJobsOptions.Value;
    }

    public void Start()
    {
        RecurringJob.AddOrUpdate("delete_old_api_calls", () => DeleteOldApiCalls(null!), _backgroundJobsOptions.EnableApiCallsCleanup ? "0 * * * *" : "0 0 31 2 *"); // execute either every hour, or never (but still allow manual triggering)
    }


    // ReSharper disable once MemberCanBePrivate.Global
    [DisableConcurrentExecution(60)]
    public async Task DeleteOldApiCalls(PerformContext context)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ApiCall>>();

        var minDate = DateTime.UtcNow.AddDays(-14);
        var outdatedApiCalls = repo.Query().Where(x => x.TimestampUtc < minDate).OrderBy(x => x.TimestampUtc);
        var outdatedApiCallsCount = await outdatedApiCalls.CountAsync();

        if (outdatedApiCallsCount > 1)
        {
            context.WriteLine($"{outdatedApiCalls} API {"call".ToQuantity(outdatedApiCallsCount)} found. Deleting ...");
            await repo.DeleteRange(outdatedApiCalls);
        }
        else
        {
            context.WriteLine($"No API calls that are older than {minDate} found.");
        }
    }
}

public static class BackgroundJobsExtensions
{
    public static void AddBackgroundJobs(this IServiceCollection services, DatabaseOptions dbOptions, BackgroundJobsOptions options)
    {
        if (options.IsEnabled)
        {
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(dbOptions.ConnectionString);
                config.UseConsole();
            });
            services.AddHangfireServer();
        }
    }

    public static void UseBackgroundJobs(this IApplicationBuilder app)
    {
        var backgroundJobsOptions = app.ApplicationServices.GetRequiredService<IOptions<BackgroundJobsOptions>>();
        if (!backgroundJobsOptions.Value.IsEnabled) return;
        var backgroundJobs = app.ApplicationServices.GetRequiredService<BackgroundJobs>();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
        backgroundJobs.Start();
    }
}