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
            var propTelemetry = (ISupportProperties)telemetry; 
            
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
            {
                //((TraceTelemetry)telemetry).Properties["SourceContext"] = sourceContext.ToString();
                // telemetry.Context.GlobalProperties["SourceContext"] = sourceContext.ToString();
                Console.WriteLine("Entered SourceContext block.");

                propTelemetry.Properties["SourceContext"] = sourceContext.ToString();
            }

            if (logEvent.Properties.TryGetValue("UserId", out var userId))
            {
                telemetry.Context.User.Id = userId.ToString();
            }
            
            if (logEvent.Properties.TryGetValue("operation_Id", out var opId))
            {
                telemetry.Context.User.Id = opId.ToString();
            }

            // trace.Properties["ActualLogLevel"] = logEvent.Level.ToString();
            // telemetry.Context.GlobalProperties["LogLevel"] = logEvent.Level.ToString();
            propTelemetry.Properties["LogLevel"] = logEvent.Level.ToString();
            
            // // find redundant properties
            // var removeProps = new[] { "UserId", "operation_parentId", "operation_Id" };
            // removeProps = removeProps.Where(prop => propTelemetry.Properties.ContainsKey(prop)).ToArray();
            //
            // foreach (var prop in removeProps)
            // {
            //     propTelemetry.Properties.Remove(prop);
            // }
            
            yield return telemetry;
        }
    }
}
