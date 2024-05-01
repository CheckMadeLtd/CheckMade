using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace CheckMade.Chat.Telegram;

public class CustomTelemetryConverter : TraceTelemetryConverter
{
    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        foreach (var telemetry in base.Convert(logEvent, formatProvider))
        {
            if (telemetry is TraceTelemetry trace)
            {
                if (logEvent.Properties.TryGetValue("SourceContext", out var value))
                {
                    trace.Properties["SourceContext"] = value.ToString();
                }
                
                if (logEvent.Properties.TryGetValue("ProcessId", out var processId))
                {
                    trace.Properties["ProcessId"] = processId.ToString();
                }
                
                trace.Properties["ActualLogLevel"] = logEvent.Level.ToString();
            }
            yield return telemetry;
        }
    }
}
