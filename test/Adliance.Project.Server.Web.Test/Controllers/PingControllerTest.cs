using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Adliance.Project.Server.Web.Test.Controllers;

public class PingControllerTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public PingControllerTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.Init();
    }

    [Fact]
    public async Task Can_Ping_Without_Authentication()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/ping");
        Assert.Equal("Pong", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Can_Ping_With_Authentication()
    {
        var client = await _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/ping");
        Assert.Equal("Authenticated Pong for API (Unit Tests)", await response.Content.ReadAsStringAsync());
    }
}