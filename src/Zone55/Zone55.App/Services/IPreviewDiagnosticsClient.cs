namespace Zone55.App.Services;

public interface IPreviewDiagnosticsClient
{
    bool Enabled { get; }
    Task WriteAsync(string source, string sessionId, string eventName, int sequence, string message);
}
