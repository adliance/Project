using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Core.Models;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string Upn { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime LastLogInUtc { get; set; }
    public DateTime CreatedUtc { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsArticlesManager { get; set; }

    private readonly List<ApiCall> _apiCalls = new();
    public IReadOnlyCollection<ApiCall> ApiCalls => _apiCalls.AsReadOnly();
}