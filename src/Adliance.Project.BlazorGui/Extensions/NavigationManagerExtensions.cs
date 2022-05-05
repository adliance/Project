using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Adliance.Project.BlazorGui.Extensions;

/// <summary>
/// Provides extensions for the <see cref="NavigationManager"/> class.
/// </summary>
public static class NavigationManagerExtensions
{
    /// <summary>
    /// Navigates to the specified URI with adding the <paramref name="queryParameters"/> as query string.
    /// </summary>
    /// <param name="navigationManager">The instance of a navigation manager.</param>
    /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI
    /// (as returned by <see cref="NavigationManager.BaseUri"/>.</param>
    /// <param name="queryParameters">The query string parameters.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new 
    /// page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    public static void NavigateTo(this NavigationManager navigationManager, string uri,
        IDictionary<string, string>? queryParameters = null, bool forceLoad = false)
    {
        navigationManager.NavigateTo(QueryHelpers.AddQueryString(uri, queryParameters), forceLoad);
    }

    /// <summary>
    /// Navigates to the specified URI with adding the current path and query as returnUrl query parameter.
    /// </summary>
    /// <param name="navigationManager">The instance of a navigation manager.</param>
    /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI
    /// (as returned by <see cref="NavigationManager.BaseUri"/>.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new 
    /// page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    public static void NavigateToWithReturnUrl(this NavigationManager navigationManager, string uri, bool forceLoad = false)
    {
        var returnUrl = new Uri(navigationManager.Uri).PathAndQuery;
        navigationManager.NavigateTo(QueryHelpers.AddQueryString(uri, "returnUrl", returnUrl), forceLoad);
    }

    /// <summary>
    /// Navigates to the specified URI of the returnUrl query parameter in the current URI.
    /// </summary>
    /// <param name="navigationManager">The instance of a navigation manager.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new 
    /// page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    public static void NavigateFromReturnUrl(this NavigationManager navigationManager, bool forceLoad = false)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        if (!QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var returnUrl))
        {
            returnUrl = "/";
        }
        navigationManager.NavigateTo(returnUrl, forceLoad);
    }
}
