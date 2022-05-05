using System.Net.Http.Headers;
using System.Net.Mime;
using Adliance.Project.BlazorGui;
using Adliance.Project.BlazorGui.Extensions;
using Adliance.Project.BlazorGui.Authentication;
using Adliance.Project.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationAndAuthorization();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Build http clients
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];

builder.Services.AddHttpClient("default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
});

var httpClientBuilder = builder.Services.AddHttpClient<IApiClient, ApiClient>("authorizedClient", client =>
{
    client.BaseAddress = new Uri(string.IsNullOrEmpty(apiBaseUrl) ? builder.HostEnvironment.BaseAddress : apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
});

builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

// In local development we have CORS enabled and need to set "credentials: include" to fetch api
if (builder.HostEnvironment.IsDevelopment())
{
    httpClientBuilder.AddHttpMessageHandler<CookieAuthenticationMessageHandler>();
    builder.Services.AddScoped<CookieAuthenticationMessageHandler>();
}

//Add custom API / business services.
builder.Services.AddServices();

//Register DevExpress
builder.Services.AddDevExpressBlazor();

await builder.Build().RunAsync();
