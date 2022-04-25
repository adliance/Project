using Adliance.Project.Server.Core.Models;

namespace Adliance.Project.Server.Core.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? ApiKeyId { get; }
    bool IsAuthenticated { get; }

    Task<string> LoadName();

    bool IsInRole(string role);
}