using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Adliance.Project.Server.Web.Test;

/// <summary>
/// A set of tests that do not test any actual functionality but is used to test the correct configuration of the authentication system in general
/// </summary>
public class AuthenticationTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public AuthenticationTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.Init();
    }

    [Theory]
    [InlineData("health")]
    [InlineData("api/ping")]
    [InlineData("swagger")]
    public async Task Can_Access_Anonymous_Endpoints(string relativeUrl)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(relativeUrl);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.NoContent });
    }


    [Theory]
    [InlineData("GET", "api/articles")]
    [InlineData("GET", "api/article/80dd3f11-45a0-4bec-94e3-58f2061fce65")] // some random GUID, does not matter as authorization triggers before any custom logic
    [InlineData("POST", "api/article")]
    [InlineData("PUT", "api/article/80dd3f11-45a0-4bec-94e3-58f2061fce65")]
    [InlineData("DELETE", "api/article/80dd3f11-45a0-4bec-94e3-58f2061fce65")]
    public async Task Can_Access_Endpoints_For_Role_ArticleManagers(string method, string relativeUrl)
    {
        await AssertAccess(method, relativeUrl, await _factory.CreateAuthenticatedClient(false, true));
    }


    /*[Theory]
    public async Task Can_Access_Endpoint_For_Role_Admin(string method, string relativeUrl)
    {
        await AssertAccess(method, relativeUrl, await _factory.CreateAuthenticatedClient(true, false));
    }*/

    private async Task AssertAccess(string method, string relativeUrl, HttpClient authenticatedClient)
    {
        // check if we can access the URL
        var authenticatedResponse = await GetResponse(method, relativeUrl, authenticatedClient);
        Assert.NotEqual(HttpStatusCode.Unauthorized, authenticatedResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, authenticatedResponse.StatusCode);

        // do the counter check if we get authorized and forbidden
        var unauthorizedClient = _factory.CreateClient();
        var unauthorizedResponse = await GetResponse(method, relativeUrl, unauthorizedClient);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        var forbiddenClient = await _factory.CreateAuthenticatedClient(false, false);
        var forbiddenResponse = await GetResponse(method, relativeUrl, forbiddenClient);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> GetResponse(string method, string relativeUrl, HttpClient client)
    {
        return method switch
        {
            "GET" => await client.GetAsync(relativeUrl),
            "PUT" => await client.PutAsJsonAsync(relativeUrl, new { }),
            "POST" => await client.PostAsJsonAsync(relativeUrl, new { }),
            "DELETE" => await client.DeleteAsync(relativeUrl),
            _ => throw new Exception($"Unknown method {method}.")
        };
    }
}