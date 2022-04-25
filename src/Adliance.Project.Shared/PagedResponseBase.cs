using System.Text.Json.Serialization;

namespace Adliance.Project.Shared;

public abstract class PagedResponseBase
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("total")] public int TotalCount { get; set; }
    [JsonPropertyName("per_page")] public int ItemsPerPage { get; set; }
    [JsonPropertyName("max_page")] public int MaxPage { get; set; }
}