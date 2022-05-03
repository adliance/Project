using Adliance.Project.Server.Web.Exceptions;
using Adliance.Project.Server.Web.ResponseFactories;
using Adliance.Project.Shared.Requests;
using Adliance.Project.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adliance.Project.Server.Web.Controllers;

[ApiController]
public class ArticleController : Controller
{
    /// <summary>
    /// Fetches a single article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article.</param>
    /// <returns>The specified article.</returns>
    /// <response code="200">Returns the specified article.</response>
    /// <response code="401">User is not authorized.</response>
    /// <response code="403">User does not have the role "Articles Manager".</response>
    /// <response code="404">The specified article ID does not exist.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.ArticlesManager), HttpGet("/api/article/{id}")]
    public async Task<ActionResult<ArticleResponse>> Get([FromServices] ArticleResponseFactory factory, [FromRoute] Guid id)
    {
        try
        {
            return await factory.Build(id);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }


    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="request">The article to create.</param>
    /// <returns>The newly created article.</returns>
    /// <response code="200">The article has been created.</response>
    /// <response code="400">One or more of the specified properties of the article are invalid.</response>
    /// <response code="401">User is not authorized.</response>
    /// <response code="403">User does not have the role "Articles Manager".</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.ArticlesManager), HttpPost("/api/article")]
    public async Task<ActionResult<ArticleResponse>> Create([FromServices] ArticleResponseFactory factory, ArticleCreateRequest request)
    {
        return await factory.Create(request);
    }


    /// <summary>
    /// Updates an existing article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article to update.</param>
    /// <param name="request">The updated article.</param>
    /// <returns>The updated article.</returns>
    /// <response code="200">The article has been updated.</response>
    /// <response code="400">One or more of the specified properties of the article are invalid.</response>
    /// <response code="401">User is not authorized.</response>
    /// <response code="403">User does not have the role "Articles Manager".</response>
    /// <response code="404">The specified article ID does not exist.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.ArticlesManager), HttpPut("/api/article/{id}")]
    public async Task<ActionResult<ArticleResponse>> Update([FromServices] ArticleResponseFactory factory, [FromRoute] Guid id, ArticleUpdateRequest request)
    {
        try
        {
            return await factory.Update(id, request);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes an existing article.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="id">The ID of the article to delete.</param>
    /// <returns></returns>
    /// <response code="200">The article has been deleted.</response>
    /// <response code="401">User is not authorized.</response>
    /// <response code="403">User does not have the role "Articles Manager".</response>
    /// <response code="404">The specified article ID does not exist.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.ArticlesManager), HttpDelete("/api/article/{id}")]
    public async Task<IActionResult> Delete([FromServices] ArticleResponseFactory factory, [FromRoute] Guid id)
    {
        try
        {
            await factory.Delete(id);
            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }
}
