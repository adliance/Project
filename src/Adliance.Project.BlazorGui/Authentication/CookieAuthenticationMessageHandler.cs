using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Adliance.Project.BlazorGui.Authentication;

/// <summary>
/// To include credentials in a cross-origin request, we need to set the credentials: include header by using the SetBrowserRequestCredentials extension method.
/// https:docs.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-5.0#httpclient-and-httprequestmessage-with-fetch-api-request-options
/// </summary>
public class CookieAuthenticationMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await base.SendAsync(request, cancellationToken);
    }
}
