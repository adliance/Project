using System.Security.Claims;
using System.Text.Encodings.Web;
using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Adliance.Project.Server.Web.Services.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IRepository<ApiKey> _repo;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IRepository<ApiKey> repo)
        : base(options, logger, encoder, clock)
    {
        _repo = repo;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? apiKey = null;
        if (Request.Query.ContainsKey(ApiKeyAuthenticationOptions.HttpQueryName) && !string.IsNullOrWhiteSpace(Request.Query[ApiKeyAuthenticationOptions.HttpQueryName]))
        {
            apiKey = Request.Query[ApiKeyAuthenticationOptions.HttpQueryName];
        }

        // HTTP header takes precedence over querystring
        if (Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.HttpHeaderName) && !string.IsNullOrWhiteSpace(Request.Headers[ApiKeyAuthenticationOptions.HttpHeaderName]))
        {
            apiKey = Request.Headers[ApiKeyAuthenticationOptions.HttpHeaderName];
        }

        if (string.IsNullOrWhiteSpace(apiKey)) return AuthenticateResult.Fail("No API key specified.");

        var matchingApiKey = await _repo.Query().SingleOrDefaultAsync(x => x.Key == apiKey);
        if (matchingApiKey == null) return AuthenticateResult.Fail("API key is invalid.");
        if (!matchingApiKey.IsEnabled) return AuthenticateResult.Fail("API key is disabled.");

        matchingApiKey.LastLogInUtc = DateTime.UtcNow;
        await _repo.Update(matchingApiKey);

        var identity = new ClaimsIdentity(ApiKeyAuthenticationOptions.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        
        identity.AddClaim(new Claim(ApiKeyAuthenticationOptions.ApiKeyIdClaimName, matchingApiKey.Id.ToString()));
        if (matchingApiKey.IsAdmin) identity.AddClaim(new Claim(ClaimTypes.Role, Roles.Admin));
        if (matchingApiKey.IsArticlesManager) identity.AddClaim(new Claim(ClaimTypes.Role, Roles.ArticlesManager));

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, ApiKeyAuthenticationOptions.AuthenticationScheme);
        return AuthenticateResult.Success(ticket);
    }

    /*protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "API key, charset=\"UTF-8\"";
        await base.HandleChallengeAsync(properties);
    }*/
}