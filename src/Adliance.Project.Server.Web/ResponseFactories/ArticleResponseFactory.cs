using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Web.Exceptions;
using Adliance.Project.Shared.Requests;
using Adliance.Project.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Adliance.Project.Server.Web.ResponseFactories;

public class ArticleResponseFactory
{
    private readonly IReadonlyRepository<Article> _readonlyRepo;
    private readonly IRepository<Article> _repo;
    private readonly ICurrentUser _currentUser;

    public ArticleResponseFactory(IReadonlyRepository<Article> readonlyRepo, IRepository<Article> repo, ICurrentUser currentUser)
    {
        _readonlyRepo = readonlyRepo;
        _repo = repo;
        _currentUser = currentUser;
    }

    
    public async Task<ArticleResponse> Build(Guid id)
    {
        var article = await Map(_readonlyRepo.Query().Where(x => x.Id == id)).SingleOrDefaultAsync();
        if (article == null) throw new EntityNotFoundException();
        return article;
    }


    public async Task<ArticleResponse> Create(ArticleCreateRequest request)
    {
        var article = await _repo.Add(new Article
        {
            Description = request.Description,
            Name = request.Name,
            LengthCm = request.LengthCm,
            UpdatedUserId = _currentUser.UserId,
            UpdatedUtc = DateTime.UtcNow,
            UpdatedApiKeyId = _currentUser.ApiKeyId
        });
        return await Build(article.Id);
    }

    public async Task<ArticleResponse> Update(Guid id, ArticleUpdateRequest request)
    {
        var article = await _repo.ById(id) ?? throw new EntityNotFoundException();
        article.Description = request.Description;
        article.Name = request.Name;
        article.LengthCm = request.LengthCm;
        article.UpdatedUtc = DateTime.UtcNow;
        article.UpdatedUserId = _currentUser.UserId;
        article.UpdatedApiKeyId = _currentUser.ApiKeyId;
        await _repo.Update(article);
        return await Build(article.Id);
    }


    public async Task Delete(Guid id)
    {
        var article = await _repo.ById(id) ?? throw new EntityNotFoundException();
        await _repo.Delete(article);
    }


    public static IQueryable<ArticleResponse> Map(IQueryable<Article> query)
    {
        return query.Select(x => new ArticleResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Updated = x.UpdatedUtc,
            LengthCm = x.LengthCm,
            UpdatedBy = x.UpdatedApiKey != null ? x.UpdatedApiKey.Name : x.UpdatedUser != null ? x.UpdatedUser.Name : "" // yeah, this IF cascade sucks, but I can't think of a better wqy
        });
    }
}