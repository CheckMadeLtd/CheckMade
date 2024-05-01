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
            // ToDo: Check if this if clause is necessary or maybe even in the way? 
            if (telemetry is TraceTelemetry trace)
            {
                if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
                {
                    // trace.Properties["SourceContext"] = sourceContext.ToString();
                    telemetry.Context.GlobalProperties["SourceContext"] = sourceContext.ToString();
                }

                if (logEvent.Properties.TryGetValue("UserId", out var userId))
                {
                    telemetry.Context.User.Id = userId.ToString();
                }
                
                if (logEvent.Properties.TryGetValue("operation_Id", out var opId))
                {
                    telemetry.Context.User.Id = opId.ToString();
                }

                // if (logEvent.Properties.TryGetValue("ProcessId", out var processId))
                // {
                //     trace.Properties["ProcessId"] = processId.ToString();
                // }
                
                // trace.Properties["ActualLogLevel"] = logEvent.Level.ToString();
                telemetry.Context.GlobalProperties["LogLevel"] = logEvent.Level.ToString();
            }
            
            // typecast to ISupportProperties so you can manipulate the properties as desired
            var propTelemetry = (ISupportProperties)telemetry;

            // find redundant properties
            var removeProps = new[] { "UserId", "operation_parentId", "operation_Id" };
            removeProps = removeProps.Where(prop => propTelemetry.Properties.ContainsKey(prop)).ToArray();

            foreach (var prop in removeProps)
            {
                propTelemetry.Properties.Remove(prop);
            }
            
            yield return telemetry;
        }
    }
}
