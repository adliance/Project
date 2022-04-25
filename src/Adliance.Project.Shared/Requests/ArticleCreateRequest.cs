using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Adliance.Project.Shared.Requests;

public class ArticleCreateRequest
{
    [Required, JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("length_cm")] public double? LengthCm { get; set; }
}