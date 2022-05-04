using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adliance.Project.Server.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous, HttpGet("/")]
    public IActionResult Index([FromQuery] string? returnUrl)
    {
        if (User.Identity is not { IsAuthenticated: true })
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action(nameof(Index), "Home", null, Request.Scheme) : returnUrl
                }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Content("Logged in.");
    }

    [AllowAnonymous, HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }
}
