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
    private readonly IHttpClientFactory _clientFactory;
    private readonly HttpClient _client;
    private readonly ILogger<HostAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

    public HostAuthenticationStateProvider(NavigationManager navigation, IHttpClientFactory clientFactory, ILogger<HostAuthenticationStateProvider> logger)
    {
        _navigationManager = navigation;
        _clientFactory = clientFactory;
        _client = _clientFactory.CreateClient("authorizedClient");
        _logger = logger;
    }

    protected Uri? ServerUrl => _client.BaseAddress;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(await GetUserAsync(useCache: true).ConfigureAwait(false));
    }

    /// <summary>
    /// Login to be called by Razor Component Authentication.razor
    /// </summary>
    public void SignIn(string? customReturnUrl = null)
    {
        var returnUrl = customReturnUrl != null ? _navigationManager.ToAbsoluteUri(customReturnUrl).ToString() : null;
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? _navigationManager.Uri);
        var logInUrl = _navigationManager.ToAbsoluteUri($"{ServerUrl}{LogInPath}?returnUrl={encodedReturnUrl}");
        _navigationManager.NavigateTo(logInUrl.ToString(), true);
    }

    /// <summary>
    /// Logout to be called by Razor Component Authentication.razor
    /// </summary>
    public void SignOut()
    {
        _navigationManager.NavigateTo($"{ServerUrl}{LogOutPath}", true);
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

        if (user.Claims != null)
        {
            foreach (var claim in user.Claims)
            {
                identity.AddClaim(new Claim(claim.Type, claim.Value));
            }
        }

        return new ClaimsPrincipal(identity);
    }
}
