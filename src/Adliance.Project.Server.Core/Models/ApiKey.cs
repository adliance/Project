using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Core.Models;

public class ApiKey : IEntity
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Notes { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime LastLogInUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public bool IsAdmin { get; set; }
    public bool IsArticlesManager { get; set; }
    
    private readonly List<ApiCall> _apiCalls = new();
    public IReadOnlyCollection<ApiCall> ApiCalls => _apiCalls.AsReadOnly();
}