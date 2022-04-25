using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Adliance.Project.Server.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, DatabaseOptions databaseOptions)
    {
        services.AddScoped(typeof(IReadonlyRepository<>), typeof(ReadonlyRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddDbContext<Database.Db>(builder => builder.UseSqlServer(databaseOptions.ConnectionString));
        services.AddScoped<MigrationsService>();
        return services;
    }
}