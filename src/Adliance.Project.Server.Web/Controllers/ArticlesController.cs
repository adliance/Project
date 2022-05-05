using Adliance.Project.Server.Web.ResponseFactories;
using Adliance.Project.Shared;
using Adliance.Project.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adliance.Project.Server.Web.Controllers;

[ApiController]
public class ArticlesController : Controller
{
    /// <summary>
    /// Fetches the articles.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="search">(Optional) If specified, returns only articles that contain this text in its name.</param>
    /// <param name="page">The page.</param>
    /// <param name="itemsPerPage">How many articles per page.</param>
    /// <returns>A paged list of articles.</returns>
    /// <response code="200">Returns the list of articles.</response>
    /// <response code="401">User is not authorized.</response>
    /// <response code="403">User does not have the role "Articles Manager".</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.ArticlesManager), HttpGet("/api/articles")]
    public async Task<ActionResult<ArticlesResponse>> Get(
        [FromServices] ArticlesResponseFactory factory,
        [FromQuery(Name = "search")] string? search = "",
        [FromQuery(Name = "page")] int? page = 1,
        [FromQuery(Name = "per-page")] int? itemsPerPage = 50)
    {
        return await factory.Build(search ?? "", page ?? 1, itemsPerPage ?? 50);
    }
}
