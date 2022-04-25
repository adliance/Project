using Adliance.Project.Server.Core.Services;
using Adliance.Project.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Adliance.Project.Server.Web.ResponseFactories;

public class ArticlesResponseFactory
{
    private readonly ArticleSearchService _articleSearchService;

    public ArticlesResponseFactory(ArticleSearchService articleSearchService)
    {
        _articleSearchService = articleSearchService;
    }

    public async Task<ArticlesResponse> Build(string search, int page, int itemsPerPage)
    {
        var query = _articleSearchService.Search(search);

        var count = await query.CountAsync();
        var maxPage = (int)Math.Ceiling((double)count / itemsPerPage);
        if (page > maxPage) page = maxPage;
        if (page <= 0) page = 1;
        if (itemsPerPage > 500) itemsPerPage = 500;

        query = query
            .OrderBy(x => x.Name)
            .Skip(itemsPerPage * (page - 1)).Take(itemsPerPage);

        return new ArticlesResponse
        {
            Page = page,
            TotalCount = count,
            ItemsPerPage = itemsPerPage,
            MaxPage = maxPage,
            Articles = await ArticleResponseFactory.Map(query).ToListAsync()
        };
    }
}