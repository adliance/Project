using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Adliance.Project.BlazorGui;

public interface IApiClient
{
    Task<TResponse> GetAllAsync<TResponse>(string path, params (string, object?)[] queryParams) where TResponse : new();

    Task GetAsync(string path, params (string, object?)[] queryParams);

    Task<TResponse> GetAsync<TResponse>(string path, params (string, object?)[] queryParams) where TResponse : new();

    async Task<TResponse> GetAsync<TResponse>(string path, Guid id, params (string, object?)[] queryParams) where TResponse : new() =>
        await GetAsync<TResponse>($"{path}/{id}", queryParams);

    Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest model) where TResponse : new();

    Task<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest model) where TResponse : new();

    async Task<TResponse> PutAsync<TRequest, TResponse>(string path, Guid id, TRequest model) where TResponse : new() =>
        await PutAsync<TRequest, TResponse>($"{path}/{id}", model);

    Task DeleteAsync(string path, Guid id);
}

public class ApiClient : IApiClient
{
    #region Constructor

    private readonly HttpClient _client;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient client, ILogger<ApiClient> logger)
    {
        _client = client;
        _logger = logger;
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    #endregion Constructor

    protected const string RootPath = "api";

    protected static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            var jso = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jso.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            return jso;
        }
    }

    protected async static Task<T> ReadFromJsonAsync<T>(HttpContent content) where T : new() =>
        await content.ReadFromJsonAsync<T>(JsonSerializerOptions) ?? new T();

    protected async static Task<T> ReadFromJsonAsync<T>(HttpResponseMessage response) where T : new()
    {
        return response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent ?
            await ReadFromJsonAsync<T>(response.Content) : new T();
    }

    /// <exception cref="ApplicationException" />
    protected async Task<bool> HandleHttpResponseStatusAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        var content = await response.Content.ReadAsStringAsync();
        var message = $"Reason: {response.ReasonPhrase}, Message: {content}";
        _logger.LogError(message);
        throw new ApplicationException(message);
    }

    protected static bool TryGetTotalCountFromHeader(HttpResponseMessage response, out int totalCount)
    {
        totalCount = default;
        if (response.Headers.TryGetValues("x-total-count", out var headerValues))
        {
            return int.TryParse(headerValues.First(), out totalCount);
        }
        return totalCount != default;
    }

    /// <summary>
    /// Converts a collection of query parameters into a <see cref="System.Collections.IDictionary"/>.
    /// </summary>
    /// <param name="queryParams">The query parameters to convert.</param>
    /// <returns>A <see cref="System.Collections.IDictionary"/> of query parameters and values.</returns>
    protected IDictionary<string, string?> ConvertQueryParameters(params (string, object?)[] queryParams)
    {
        var query = new Dictionary<string, string?>();

        foreach (var param in queryParams.Where(x => x.Item2 != null))
        {
            query[param.Item1] = param.Item2?.ToString();
        }

        return query;
    }

    protected static string BuildUrl(string path, IDictionary<string, string?>? queryParams) =>
        QueryHelpers.AddQueryString($"{RootPath}/{path}", queryParams);

    public async Task<TResponse> GetAllAsync<TResponse>(string path, params (string, object?)[] queryParams) where TResponse : new()
    {
        var query = ConvertQueryParameters(queryParams);
        var response = await _client.GetAsync(BuildUrl(path, query));
        await HandleHttpResponseStatusAsync(response);
        return await ReadFromJsonAsync<TResponse>(response.Content);
    }

    public async Task GetAsync(string path, params (string, object?)[] queryParams)
    {
        var query = ConvertQueryParameters(queryParams);
        var response = await _client.GetAsync(BuildUrl(path, query));
        await HandleHttpResponseStatusAsync(response);
    }

    public async Task<TResponse> GetAsync<TResponse>(string path, params (string, object?)[] queryParams) where TResponse : new()
    {
        var query = ConvertQueryParameters(queryParams);
        var response = await _client.GetAsync(BuildUrl(path, query));
        var isSuccess = await HandleHttpResponseStatusAsync(response);
        return isSuccess ? await ReadFromJsonAsync<TResponse>(response) : new TResponse();
    }

    public async Task<TResponse> GetAsync<TResponse>(string path, Guid id, params (string, object?)[] queryParams) where TResponse : new()
    {
        return await GetAsync<TResponse>($"{path}/{id}", queryParams);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest model) where TResponse : new()
    {
        var response = await _client.PostAsJsonAsync($"{RootPath}/{path}", model, JsonSerializerOptions);
        var isSuccess = await HandleHttpResponseStatusAsync(response);
        return isSuccess ? await ReadFromJsonAsync<TResponse>(response.Content) : new TResponse();
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest model) where TResponse : new()
    {
        var response = await _client.PutAsJsonAsync($"{RootPath}/{path}", model, JsonSerializerOptions);
        var isSuccess = await HandleHttpResponseStatusAsync(response);
        return isSuccess ? await ReadFromJsonAsync<TResponse>(response.Content) : new TResponse();
    }

    public async Task DeleteAsync(string path, Guid id)
    {
        var response = await _client.DeleteAsync($"{RootPath}/{path}/{id}");
        await HandleHttpResponseStatusAsync(response);
    }
}
