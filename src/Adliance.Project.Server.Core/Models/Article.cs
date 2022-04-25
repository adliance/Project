using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Core.Models;

public class Article : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public double? LengthCm { get; set; }

    public DateTime UpdatedUtc { get; set; }
    public Guid? UpdatedApiKeyId { get; set; }
    public Guid? UpdatedUserId { get; set; }

    public ApiKey? UpdatedApiKey { get; set; }
    public User? UpdatedUser { get; set; }
}