using System.Threading.Tasks;
using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Adliance.Project.Server.Web.Test.Middleware;

public class ApiCallMiddlewareTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public ApiCallMiddlewareTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.Init();
    }

    [Fact]
    public async Task Anonymous_Api_Call_Is_Logged_In_Database()
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReadonlyRepository<ApiCall>>();
        Assert.Empty(repo.Query());
        await client.GetAsync("/api/ping");

        Assert.InRange(await repo.Query().CountAsync(), 1, 1);
        var apiCall = await repo.Query().SingleAsync();
        Assert.Equal("/api/ping", apiCall.Url);
        Assert.Null(apiCall.UserId);
        Assert.Null(apiCall.ApiKeyId);
    }

    [Fact]
    public async Task Authenticated_Api_Call_Is_Logged_In_Database()
    {
        var client = await _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReadonlyRepository<ApiCall>>();
        Assert.Empty(repo.Query());
        await client.GetAsync("/api/ping");

        Assert.InRange(await repo.Query().CountAsync(), 1, 1);
        var apiCall = await repo.Query().SingleAsync();
        Assert.Equal("/api/ping", apiCall.Url);
        Assert.Null(apiCall.UserId);
        Assert.NotNull(apiCall.ApiKeyId);
    }

    [Fact]
    public async Task Authenticated_Api_Call_Updates_Api_Key_Last_Logged_In_Date()
    {
        var client = await _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReadonlyRepository<ApiKey>>();
        await client.GetAsync("/api/ping");

        var apiKey = await repo.Query().SingleAsync();
        AssertMore.IsNow(apiKey.LastLogInUtc);
    }

    [Fact]
    public async Task Disabled_Api_Key_Will_Not_Authenticate()
    {
        var client = await _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();

        var response = await client.GetAsync("/api/ping");
        Assert.NotEqual("Pong", await response.Content.ReadAsStringAsync());

        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ApiKey>>();
        var apiKey = await repo.Query().SingleAsync();
        apiKey.IsEnabled = false;
        await repo.Update(apiKey);

        response = await client.GetAsync("/api/ping");
        Assert.Equal("Pong", await response.Content.ReadAsStringAsync());
    }
}