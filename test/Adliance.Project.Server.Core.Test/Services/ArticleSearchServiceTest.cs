using System.Collections.Generic;
using System.Linq;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Core.Services;
using Xunit;

namespace Adliance.Project.Server.Core.Test.Services;

public class ArticleSearchServiceTest
{
    private readonly ArticleSearchService _service;

    public ArticleSearchServiceTest()
    {
        var list = new List<Article>
        {
            new() { Name = "Article 1" },
            new() { Name = "Article 2", Description = "Description 2" },
            new() { Name = "Article 3", Description = "Description 3" }
        };

        _service = new ArticleSearchService(new MockedReadonlyRepository<Article>(list));
    }

    [Fact]
    public void Can_Search_For_Articles_By_Name()
    {
        var result = _service.Search("Article 1").ToList();
        Assert.Single(result);
        Assert.Equal("Article 1", result.Single().Name);
    }

    [Fact]
    public void Can_Search_For_Articles_By_Description()
    {
        var result = _service.Search("Description 3").ToList();
        Assert.Single(result);
        Assert.Equal("Article 3", result.Single().Name);
    }

    [Fact]
    public void Empty_Search_Termin_Will_Return_All()
    {
        var result = _service.Search("").ToList();
        Assert.Equal(3, result.Count);
    }
}