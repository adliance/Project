using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Core.Models;

public class ApiCall : IEntity
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public Guid? ApiKeyId { get; set; }
    public Guid? UserId { get; set; }
    
    public string? Url { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string RequestMethod { get; set; } = "";
    public int ResponseCode { get; set; }
    public long DurationMs { get; set; }
    
    public ApiKey? ApiKey { get; set; }
    public User? User { get; set; }
}