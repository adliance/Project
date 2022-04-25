using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;

namespace Adliance.Project.Server.Core.Services;

public class ArticleSearchService
{
    private readonly IReadonlyRepository<Article> _repo;

    public ArticleSearchService(IReadonlyRepository<Article> repo)
    {
        _repo = repo;
    }

    public IQueryable<Article> Search(string? searchTerm)
    {
        return Search(_repo.Query(), searchTerm);
    }

    public IQueryable<Article> Search(IQueryable<Article> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm) || (x.Description != null && x.Description.Contains(searchTerm)));
        }
        return query;
    }
}