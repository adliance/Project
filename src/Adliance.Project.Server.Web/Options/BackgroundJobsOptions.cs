namespace Adliance.Project.Server.Web.Options;

public class BackgroundJobsOptions
{
    public const string BackgroundJobs = "BackgroundJobs";

    public bool EnableApiCallsCleanup { get; set; }
    public bool IsEnabled => EnableApiCallsCleanup;
}
