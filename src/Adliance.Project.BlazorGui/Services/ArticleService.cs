using Adliance.Project.Shared.Requests;
using Adliance.Project.Shared.Responses;

namespace Adliance.Project.BlazorGui.Services;

public class ArticleService
{
    private readonly IApiClient _apiClient;

    public ArticleService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private const string Path = "article";

    /// <summary>
    /// Fetches the articles.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="search">(Optional) If specified, returns only articles that contain this text in its name.</param>
    /// <param name="page">The page.</param>
    /// <param name="itemsPerPage">How many articles per page.</param>
    /// <returns>A paged list of articles.</returns>
    public async Task<ArticlesResponse> GetAllAsync(string? search = "", int? page = 1, int? itemsPerPage = 50) =>
        await _apiClient.GetAllAsync<ArticlesResponse>($"{Path}s", 
            (nameof(search), search), (nameof(page), page), (nameof(itemsPerPage), itemsPerPage));

    /// <summary>
    /// Fetches a single article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article.</param>
    /// <returns>The specified article.</returns>
    public async Task<ArticleResponse> GetAsync(Guid id) =>
        await _apiClient.GetAsync<ArticleResponse>(Path, id);

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="request">The article to create.</param>
    /// <returns>The newly created article.</returns>
    public async Task<ArticleResponse> CreateAsync(ArticleCreateRequest request) =>
        await _apiClient.PostAsync<ArticleCreateRequest, ArticleResponse>(Path, request);

    /// <summary>
    /// Updates an existing article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article to update.</param>
    /// <param name="request">The updated article.</param>
    /// <returns>The updated article.</returns>
    public async Task<ArticleResponse> UpdateAsync(Guid id, ArticleUpdateRequest request) =>
        await _apiClient.PutAsync<ArticleUpdateRequest, ArticleResponse>(Path, id, request);

    /// <summary>
    /// Deletes an existing article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article to delete.</param>
    /// <returns></returns>
    public async Task DeleteAsync(Guid id) =>
        await _apiClient.DeleteAsync(Path, id);
}
