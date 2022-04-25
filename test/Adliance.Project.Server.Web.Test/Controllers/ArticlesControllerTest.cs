using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Shared.Requests;
using Adliance.Project.Shared.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Adliance.Project.Server.Web.Test.Controllers;

public class ArticlesControllerTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public ArticlesControllerTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.Init();
    }

    [Fact]
    public async Task Can_Fetch_Paged_Articles()
    {
        var client = await _factory.CreateAuthenticatedClient();
        var response = await client.GetFromJsonAsync<ArticlesResponse>("/api/articles") ?? throw new Exception("Unable to deserialize.");
        Assert.Empty(response.Articles);
        Assert.Equal(0, response.MaxPage);
        Assert.Equal(1, response.Page);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(50, response.ItemsPerPage);

        await AddArticle("Article 1", null);
        await AddArticle("Article 2", null);

        response = await client.GetFromJsonAsync<ArticlesResponse>("/api/articles?page=1&per-page=1") ?? throw new Exception("Unable to deserialize.");
        Assert.Equal(1, response.Articles.Count);
        Assert.Equal(2, response.MaxPage);
        Assert.Equal(1, response.Page);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.ItemsPerPage);
        Assert.Equal("Article 1", response.Articles.Single().Name);

        response = await client.GetFromJsonAsync<ArticlesResponse>("/api/articles?page=2&per-page=1") ?? throw new Exception("Unable to deserialize.");
        Assert.Equal(1, response.Articles.Count);
        Assert.Equal(2, response.MaxPage);
        Assert.Equal(2, response.Page);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.ItemsPerPage);
        Assert.Equal("Article 2", response.Articles.Single().Name);
    }

    [Fact]
    public async Task Can_Search_For_Articles()
    {
        var client = await _factory.CreateAuthenticatedClient();
        await AddArticle("Article 1 searchterm", null);
        await AddArticle("Article 2", "searchterm");
        await AddArticle("Article 3", null);

        var response = await client.GetFromJsonAsync<ArticlesResponse>("/api/articles?search=searchterm") ?? throw new Exception("Unable to deserialize.");
        Assert.Equal(2, response.Articles.Count);
        Assert.Equal(1, response.MaxPage);
        Assert.Equal(1, response.Page);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(50, response.ItemsPerPage);
        Assert.Equal("Article 1 searchterm", response.Articles.First().Name);
        Assert.Equal("Article 2", response.Articles.Skip(1).First().Name);
    }
    
    [Fact]
    public async Task Will_Allow_Max_500_Items_Per_Page()
    {
        var client = await _factory.CreateAuthenticatedClient();
        var response = await client.GetFromJsonAsync<ArticlesResponse>("/api/articles?per-page=1000") ?? throw new Exception("Unable to deserialize.");
        Assert.Equal(500, response.ItemsPerPage);
    }

    private async Task AddArticle(string name, string? description)
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Article>>();
        await repo.Add(new Article
        {
            Description = description,
            Name = name
        });
    }
}