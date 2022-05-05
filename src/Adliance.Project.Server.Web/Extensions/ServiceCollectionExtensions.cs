using System.Reflection;
using System.Security.Claims;
using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Server.Core.Models;
using Adliance.Project.Server.Infrastructure.Database;
using Adliance.Project.Server.Web.Options;
using Adliance.Project.Server.Web.ResponseFactories;
using Adliance.Project.Server.Web.Services;
using Adliance.Project.Server.Web.Services.Authentication;
using Adliance.Project.Server.Web.Services.BackgroundJobs;
using Adliance.Project.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Adliance.Project.Server.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddResponseFactories(this IServiceCollection services)
    {
        services.AddTransient<ArticleResponseFactory>();
        services.AddTransient<ArticlesResponseFactory>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<BackgroundJobs>();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }

    public static void AddOptions(this IServiceCollection services, IConfiguration configuration, out DatabaseOptions dbOptions, out AzureAdOptions azureAdOptions, out BackgroundJobsOptions backgroundJobsOptions)
    {
        azureAdOptions = configuration.GetSection(AzureAdOptions.AzureAd).Get<AzureAdOptions>();
        dbOptions = configuration.GetSection(DatabaseOptions.Database).Get<DatabaseOptions>();

        backgroundJobsOptions = configuration.GetSection(BackgroundJobsOptions.BackgroundJobs).Get<BackgroundJobsOptions>();
        services.Configure<BackgroundJobsOptions>(configuration.GetSection(BackgroundJobsOptions.BackgroundJobs));
    }

    public static void AddAuthenticationAndAuthorization(this IServiceCollection services, AzureAdOptions azureAdOptions)
    {
        services.AddAuthorization();
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "smart";
                options.DefaultChallengeScheme = "smart";
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme("smart", "API Key, or Cookie", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (!string.IsNullOrWhiteSpace(ApiKeyAuthenticationOptions.HttpQueryName) && context.Request.Query.ContainsKey(ApiKeyAuthenticationOptions.HttpQueryName))
                    {
                        return ApiKeyAuthenticationOptions.AuthenticationScheme;
                    }

                    if (!string.IsNullOrWhiteSpace(ApiKeyAuthenticationOptions.HttpHeaderName) && context.Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.HttpHeaderName))
                    {
                        return ApiKeyAuthenticationOptions.AuthenticationScheme;
                    }

                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        // API calls should return error 401, no redirect to Azure AD login. But we still want it to work from Swagger without API key if the user is already logged in.
                        return context.Request.Cookies.ContainsKey(Names.AuthenticationCookieName)
                            ? CookieAuthenticationDefaults.AuthenticationScheme
                            : ApiKeyAuthenticationOptions.AuthenticationScheme;
                    }

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddOpenIdConnect(options =>
            {
                options.ClientId = azureAdOptions.ClientId;
                options.ClientSecret = azureAdOptions.ClientSecret;
                options.SignedOutRedirectUri = "/";
                options.Authority = azureAdOptions.Authority;
                options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false }; // required, or the multi-tenant registering flow will fail
                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = context =>
                    {
                        var upn =
                            context.Principal?.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value ??
                            context.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Upn)?.Value
                            ?? context.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
                            ?? "";

                        if (string.IsNullOrWhiteSpace(upn)) throw new Exception("No UPN available.");

                        // please not that we automatically accept every user that comes in, we could do a more specific filtering here

                        var scope = context.HttpContext.RequestServices.CreateScope();
                        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
                        var matchingUser = userRepo.Query().SingleOrDefault(x => x.Upn.ToLower() == upn.ToLower());

                        if (matchingUser == null)
                        {
                            matchingUser = new User
                            {
                                CreatedUtc = DateTime.UtcNow,
                                Upn = upn,
                                Name = upn
                            };
                            userRepo.Add(matchingUser).GetAwaiter().GetResult();
                        }

                        matchingUser.LastLogInUtc = DateTime.UtcNow;
                        userRepo.Update(matchingUser);

                        // remove all azure claims, to make the cookie smaller
                        var identity = context.Principal?.Identities.First();
                        if (identity != null)
                        {
                            foreach (var c in identity.Claims.ToList())
                            {
                                identity.RemoveClaim(c);
                            }

                            identity.AddClaim(new Claim(ClaimTypes.Name, matchingUser.Name));
                            identity.AddClaim(new Claim(ApiKeyAuthenticationOptions.UserIdClaimName, matchingUser.Id.ToString()));
                            if (matchingUser.IsAdmin) identity.AddClaim(new Claim(ClaimTypes.Role, Roles.Admin));
                            if (matchingUser.IsArticlesManager) identity.AddClaim(new Claim(ClaimTypes.Role, Roles.ArticlesManager));
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.Name = Names.AuthenticationCookieName;
                    options.LoginPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                    options.AccessDeniedPath = "/error/403";
                }
            )
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.AuthenticationScheme, _ => { });
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyAuthenticationOptions.HttpHeaderName
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "apiKey"
                        }
                    },
                    new List<string>()
                }
            });

            var version = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            options.SwaggerDoc("api", new OpenApiInfo
            {
                Title = Names.ApplicationName,
                Version = $"v{version}",
                Contact = new OpenApiContact
                {
                    Name = "Hannes Sachsenhofer",
                    Email = "hannes.sachsenhofer@adliance.net"
                },
                Description = $"API to access {Names.ApplicationName} functionality."
            });
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });
    }
}
