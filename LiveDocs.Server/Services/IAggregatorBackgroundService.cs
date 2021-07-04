namespace LiveDocs.Server.Services
{
    public interface IAggregatorBackgroundService
    {
        string GetLatestMarkdown(string resourceName);
    }
}