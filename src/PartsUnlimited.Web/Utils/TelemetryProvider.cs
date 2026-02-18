using Microsoft.ApplicationInsights;
using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

public interface ITelemetryProvider
{
    void TrackTrace(string message);
    void TrackEvent(string eventName);
    void TrackException(Exception ex);
    void TrackPageView(string pageName);
}

public class TelemetryProvider : ITelemetryProvider
{
    private readonly TelemetryClient _client;

    public TelemetryProvider(TelemetryClient client) => _client = client;

    public void TrackTrace(string message) => _client.TrackTrace(message);
    public void TrackEvent(string eventName) => _client.TrackEvent(eventName);
    public void TrackException(Exception ex) => _client.TrackException(ex);
    public void TrackPageView(string pageName) => _client.TrackPageView(pageName);
}
