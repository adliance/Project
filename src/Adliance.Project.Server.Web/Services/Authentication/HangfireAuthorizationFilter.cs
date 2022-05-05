using Adliance.Project.Server.Core.Interfaces;
using Adliance.Project.Shared;
using Hangfire.Dashboard;

namespace Adliance.Project.Server.Web.Services.Authentication;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext()?.RequestServices.GetRequiredService<ICurrentUser>();
        return user is { IsAuthenticated: true } && user.IsInRole(Roles.Admin);
    }
}
