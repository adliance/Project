using System.Text.Json.Serialization;

namespace Adliance.Project.Shared.Responses;

public class ArticleResponse
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("length_cm")] public double? LengthCm { get; set; }

    [JsonPropertyName("updated")] public DateTime Updated { get; set; }
    [JsonPropertyName("updated_by")] public string UpdatedBy { get; set; } = "";
}