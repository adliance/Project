using System.Net.Http.Json;
using System.Security.Claims;
using Adliance.Project.Shared.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Adliance.Project.BlazorGui.Authentication;

/// <inheritdoc />
public class HostAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private const string LogInPath = "";
    private const string LogOutPath = "logout";

    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _client;
    private readonly ILogger<HostAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public HostAuthenticationStateProvider(NavigationManager navigation, IHttpClientFactory clientFactory,
        ILogger<HostAuthenticationStateProvider> logger)
    {
        _navigationManager = navigation;
        _client = clientFactory.CreateClient("authorizedClient");
        _logger = logger;
    }

    private Uri? ServerUrl => _client.BaseAddress;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(await GetUserAsync(useCache: true).ConfigureAwait(false));
    }

    /// <summary>
    /// Login to be called by Razor Component Authentication.razor
    /// </summary>
    public void SignIn(string? customReturnUrl = null)
    {
        NavigateTo($"{ServerUrl}{LogInPath}", BuildEncodedReturnUrl(customReturnUrl));
    }

    /// <summary>
    /// Logout to be called by Razor Component Authentication.razor
    /// </summary>
    public void SignOut(string? customReturnUrl = null)
    {
        NavigateTo($"{ServerUrl}{LogOutPath}", BuildEncodedReturnUrl(customReturnUrl));
    }
    
    private void NavigateTo(string url, string returnUrl)
    {
        var navigateUrl = _navigationManager.ToAbsoluteUri($"{url}?returnUrl={returnUrl}");
        _navigationManager.NavigateTo(navigateUrl.ToString(), true);
    }

    private string BuildEncodedReturnUrl(string? customReturnUrl = null)
    {
        var returnUrl = customReturnUrl != null ? _navigationManager.ToAbsoluteUri(customReturnUrl).ToString() : null;
        return Uri.EscapeDataString(returnUrl ?? _navigationManager.Uri);
    }
    
    private async ValueTask<ClaimsPrincipal> GetUserAsync(bool useCache = false)
    {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
        {
            _logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        _logger.LogDebug("Fetching user");
        _cachedUser = await FetchUserAsync().ConfigureAwait(false);
        _userLastCheck = now;

        return _cachedUser;
    }

    private async Task<ClaimsPrincipal> FetchUserAsync()
    {
        UserInfo? user = null;

        try
        {
            _logger.LogInformation(_client.BaseAddress?.ToString());
            user = await _client.GetFromJsonAsync<UserInfo>("api/user").ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            _logger.LogWarning(exc, "Fetching user failed.");
        }

        if (user == null || !user.IsAuthenticated)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var identity = new ClaimsIdentity(
            nameof(HostAuthenticationStateProvider),
            user.NameClaimType,
            user.RoleClaimType);

        foreach (var claim in user.Claims)
        {
            identity.AddClaim(new Claim(claim.Type, claim.Value));
        }

        return new ClaimsPrincipal(identity);
    }
}
