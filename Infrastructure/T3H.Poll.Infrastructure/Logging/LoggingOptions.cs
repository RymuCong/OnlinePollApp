﻿namespace T3H.Poll.Infrastructure.Logging;

public class LoggingOptions
{
    public Dictionary<string, string> LogLevel { get; set; }

    public FileOptions File { get; set; }

    public EventLogOptions EventLog { get; set; }

    public ApplicationInsightsOptions ApplicationInsights { get; set; }

    public OpenTelemetryOptions OpenTelemetry { get; set; }
}
