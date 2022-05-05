namespace Adliance.Project.Server.Web.Options;

public class AzureAdOptions
{
    public const string AzureAd = "AzureAd";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Authority { get; set; } = "";
}
