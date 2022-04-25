using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Web.Services.Authentication;

namespace Adliance.Project.Server.Web.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IReadonlyRepository<User> _userRepo;
    private readonly IReadonlyRepository<ApiKey> _apiKeyRepo;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, IReadonlyRepository<User> userRepo, IReadonlyRepository<ApiKey> apiKeyRepo)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepo = userRepo;
        _apiKeyRepo = apiKeyRepo;
    }

    public Guid? UserId => GetGuidFromClaim(ApiKeyAuthenticationOptions.UserIdClaimName);
    public Guid? ApiKeyId => GetGuidFromClaim(ApiKeyAuthenticationOptions.ApiKeyIdClaimName);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true && (ApiKeyId.HasValue || UserId.HasValue);

    public async Task<string> LoadName()
    {
        if (UserId.HasValue)
        {
            return (await _userRepo.ById(UserId.Value))?.Name ?? throw new Exception("User is authenticated, but cannot be found in database.");
        }

        if (ApiKeyId.HasValue)
        {
            return (await _apiKeyRepo.ById(ApiKeyId.Value))?.Name ?? throw new Exception("API key is authenticated, but cannot be found in database.");
        }

        throw new Exception("No user or API key authenticated");
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
    }

    private Guid? GetGuidFromClaim(string claimType)
    {
        var claim = _httpContextAccessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == claimType);
        if (claim != null && Guid.TryParse(claim.Value, out var id)) return id;
        return null;
    }
}