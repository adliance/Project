using System.Diagnostics;
using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace Adliance.Project.Server.Web.Middleware;

public class ApiCallsMiddleware
{
    private readonly RequestDelegate _next;

    public ApiCallsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        await using var requestBodyStream = new MemoryStream();
        await using var responseBodyStream = new MemoryStream();
        var originalRequestBody = context.Request.Body;
        context.Request.EnableBuffering();
        var originalResponseBody = context.Response.Body;

        try
        {
            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            string requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;
            context.Response.Body = responseBodyStream;

            var watch = Stopwatch.StartNew();
            await _next(context);
            watch.Stop();

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();

            var currentUser = context.RequestServices.GetRequiredService<ICurrentUser>();
            var repo = context.RequestServices.GetRequiredService<IRepository<ApiCall>>();

            await repo.Add(new ApiCall
            {
                TimestampUtc = DateTime.UtcNow,
                RequestBody = requestBodyText,
                ResponseBody = responseBodyText,
                Url = context.Request.GetUri().PathAndQuery,
                ApiKeyId = currentUser.ApiKeyId,
                UserId = currentUser.UserId,
                RequestMethod = context.Request.Method,
                ResponseCode = context.Response.StatusCode,
                DurationMs = watch.ElapsedMilliseconds,
            });

            responseBodyStream.Seek(0, SeekOrigin.Begin);

            await responseBodyStream.CopyToAsync(originalResponseBody);
        }
        finally
        {
            context.Request.Body = originalRequestBody;
            context.Response.Body = originalResponseBody;
        }
    }
}