using Adliance.Project.Server.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adliance.Project.Server.Web.Controllers;

[ApiController]
public class PingController : Controller
{
    /// <summary>
    /// Just a simple ping, to see if the API is up and running and that authentication is working.
    /// </summary>
    /// <returns>Returns the string "Pong".</returns>
    /// <response code="200">Everything is working as expected.</response>
    /// <response code="401">Authentication is not working correctly.</response> 
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
