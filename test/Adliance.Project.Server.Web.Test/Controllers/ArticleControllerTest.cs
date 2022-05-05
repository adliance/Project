using System;
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

public class ArticleControllerTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public ArticleControllerTest(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.Init();
    }

    [Fact]
    public async Task Can_Create_Update_Delete_Article()
    {
        var client = await _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReadonlyRepository<Article>>();
        Assert.Empty(await repo.Query().ToListAsync());

        // create a new article
        var response = await client.PostAsJsonAsync("/api/article", new ArticleCreateRequest
        {
            Description = "My Description.",
            Name = "My Name.",
            LengthCm = 123
        });

        // check the response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var article = await response.Content.ReadFromJsonAsync<ArticleResponse>() ?? throw new Exception("Unable to deserialize.");
        Assert.Equal("My Description.", article.Description);
        Assert.Equal("My Name.", article.Name);
        Assert.Equal(123, article.LengthCm);
        Assert.Equal("API (Unit Tests)", article.UpdatedBy);
        AssertMore.IsNow(article.Updated);

        // but also check the database
        AssertArticlesAreEqual(article, await repo.Query().SingleAsync());

        // update the article
        response = await client.PutAsJsonAsync($"/api/article/{article.Id}", new ArticleUpdateRequest
        {
            Description = "My updated Description.",
            Name = "My updated Name.",
            LengthCm = null
        });

        // check the updated response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        article = await response.Content.ReadFromJsonAsync<ArticleResponse>() ?? throw new Exception("Unable to deserialize.");
        Assert.Equal("My updated Description.", article.Description);
        Assert.Equal("My updated Name.", article.Name);
        Assert.Null(article.LengthCm);
        Assert.Equal("API (Unit Tests)", article.UpdatedBy);
        AssertMore.IsNow(article.Updated);

        // but also check the updated database
        AssertArticlesAreEqual(article, await repo.Query().SingleAsync());
        
        // delete the article
        response = await client.DeleteAsync($"/api/article/{article.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await repo.Query().ToListAsync());
    }

    [Fact]
    public async Task Create_Or_Update_Without_Name_Throws_400()
    {
        var client = await _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/article", new ArticleCreateRequest { Name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        response = await client.PutAsJsonAsync($"/api/article/{Guid.NewGuid()}", new ArticleCreateRequest { Name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_Or_Delete_Of_Invalid_Id_Throws_404()
    {
        var client = await _factory.CreateAuthenticatedClient();
       
        var response = await client.PutAsJsonAsync($"/api/article/{Guid.NewGuid()}", new ArticleCreateRequest { Name = "Name" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await client.DeleteAsync($"/api/article/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    private void AssertArticlesAreEqual(ArticleResponse article, Article articleInDb)
    {
        Assert.Null(articleInDb.UpdatedUserId);
        Assert.NotNull(articleInDb.UpdatedApiKeyId);
        AssertMore.IsNow(articleInDb.UpdatedUtc);
        Assert.Equal(article.Name, articleInDb.Name);
        Assert.Equal(article.Description, articleInDb.Description);
        Assert.Equal(article.LengthCm, articleInDb.LengthCm);
        Assert.Equal(article.Id, articleInDb.Id);
    }
}