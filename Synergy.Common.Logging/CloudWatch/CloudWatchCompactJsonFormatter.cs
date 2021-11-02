using System;
using System.Globalization;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Synergy.Common.Logging.CloudWatch
{
    public class CloudWatchCompactJsonFormatter : ITextFormatter
    {
        private readonly JsonValueFormatter _valueFormatter;

        public CloudWatchCompactJsonFormatter(JsonValueFormatter valueFormatter = null)
        {
            this._valueFormatter = valueFormatter ?? new JsonValueFormatter("$type");
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.Write("{\"Message\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            output.Write(",\"MessageTypeId\":\"");
            var id = EventIdHash.Compute(logEvent.MessageTemplate.Text);
            output.Write(id.ToString("x8", CultureInfo.InvariantCulture));

            output.Write('"');
            output.Write(",\"Level\":\"");
            output.Write(logEvent.Level);
            output.Write('\"');

            if (logEvent.Exception != null)
            {
                output.Write(",\"Exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                this._valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
            output.WriteLine();
        }

        private static class EventIdHash
        {
            public static uint Compute(string messageTemplate)
            {
                if (messageTemplate == null)
                {
                    throw new ArgumentNullException(nameof(messageTemplate));
                }

                unchecked
                {
                    uint hash = 0;
                    foreach (var b in messageTemplate)
                    {
                        hash += b;
                        hash += hash << 10;
                        hash ^= hash >> 6;
                    }

                    hash += hash << 3;
                    hash ^= hash >> 11;
                    hash += hash << 15;

                    return hash;
                }
            }
        }
    }
}