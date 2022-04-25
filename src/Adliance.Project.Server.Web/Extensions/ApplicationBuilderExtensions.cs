using Microsoft.AspNetCore.HttpOverrides;

namespace Adliance.Project.Server.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseHttps(this IApplicationBuilder app)
    {
        // support reverse proxies, for example this is required the get the actual user host behind a load balancer like Azure AppService
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            RequireHeaderSymmetry = false,
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        };
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    public static void UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseStatusCodePages();
    }
    
    public static void UseSwaggerAndSwaggerUi(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/api/swagger.json", $"{Names.ApplicationName} API");
        });
    }
}