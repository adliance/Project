using System.Text.Json.Serialization;

namespace Adliance.Project.Shared.Responses;

public class ArticlesResponse : PagedResponseBase
{
    [JsonPropertyName("articles")] public IList<ArticleResponse> Articles { get; set; } = new List<ArticleResponse>();
}