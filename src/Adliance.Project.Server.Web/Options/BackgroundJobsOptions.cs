namespace Adliance.Project.Server.Web.Options;

public class BackgroundJobsOptions
{
    public bool EnableApiCallsCleanup { get; set; }
    public bool IsEnabled => EnableApiCallsCleanup;
}