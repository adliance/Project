using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adliance.Project.Server.Infrastructure.Database;

public class MigrationsService
{
    private readonly Db _db;
    private readonly ILogger<MigrationsService> _logger;

    public MigrationsService(Db db, ILogger<MigrationsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RunMigrations()
    {
        try
        {
            var pendingMigrationsCount = (await _db.Database.GetPendingMigrationsAsync()).Count();
            if (pendingMigrationsCount > 0)
            {
                _logger.LogWarning($"Applying {pendingMigrationsCount} migration(s) ...");
                _db.Database.SetCommandTimeout(60 * 10); // 10m timeout
                await _db.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            // we ignore when migrations fail, because we run migrations an application startup and failing migrations would otherwise break application startup
            _logger.LogCritical(ex, "Unable to run migrations.");
        }
    }
}