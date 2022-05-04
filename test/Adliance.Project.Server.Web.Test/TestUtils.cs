using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Core.Services;
using Adliance.Project.Server.Infrastructure.Database;
using Adliance.Project.Server.Web.Options;
using Adliance.Project.Server.Web.Services.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)] // to avoid concurrent execution of tests, as this would wreak havoc with our automatic migrations

namespace Adliance.Project.Server.Web.Test;

public static class TestUtils
{
    public static WebApplicationFactory<Startup> Init(this WebApplicationFactory<Startup> factory)
    {
        factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        [$"{nameof(BackgroundJobsOptions.BackgroundJobs)}:{nameof(BackgroundJobsOptions.EnableApiCallsCleanup)}"] = "false",
                        [$"{nameof(DatabaseOptions.Database)}:{nameof(DatabaseOptions.ConnectionString)}"] = "Data Source=localhost; Initial Catalog=unittests; User ID=sa; Password=P4ss.W0rd; MultipleActiveResultSets=False;"
                    });
                configBuilder.AddEnvironmentVariables();
            });
        });

        // each test should run with an empty database, so that we can be sure of the data present in the database
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Db>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();

        return factory;
    }

    public static async Task<HttpClient> CreateAuthenticatedClient(this WebApplicationFactory<Startup> factory, bool isAdmin = true, bool isArticlesManager = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Db>();
        var apiKey = new ApiKey
        {
            Name = "API (Unit Tests)",
            Key = Crypto.RandomString(50),
            IsEnabled = true,
            IsAdmin = isAdmin,
            IsArticlesManager = isArticlesManager,
            CreatedUtc = DateTime.UtcNow,
            Notes = "Automatically created as part of an integration test."
        };
        await db.AddAsync(apiKey);
        await db.SaveChangesAsync();


        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationOptions.HttpHeaderName, apiKey.Key);
        return client;
    }

    /*
    public static JsonSerializerOptions JsonOptions
    {
        get
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }*/
}
