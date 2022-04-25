using Microsoft.AspNetCore.Authentication;

namespace Adliance.Project.Server.Web.Services.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string HttpHeaderName = "x-api-key";
    public const string HttpQueryName = "key";
    public const string AuthenticationScheme = "API-Key";

    public const string ApiKeyIdClaimName = "ApiKeyId";
    public const string UserIdClaimName = "UserId";
}