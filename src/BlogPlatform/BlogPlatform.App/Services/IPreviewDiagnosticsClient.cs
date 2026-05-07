namespace BlogPlatform.App.Services
{
    public interface IPreviewDiagnosticsClient
    {
        Task WriteAsync(string source, string eventName, int sequence, string message);
    }
}