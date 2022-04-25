using Adliance.Project.Server.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adliance.Project.Server.Web.Controllers;

[ApiController]
public class PingController : Controller
{
    [AllowAnonymous, HttpGet("/api/ping")]
    public async Task<ActionResult<string>> Ping([FromServices] ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            return $"Authenticated Pong for {await currentUser.LoadName()}";
        }

        return Content("Pong");
    }
}