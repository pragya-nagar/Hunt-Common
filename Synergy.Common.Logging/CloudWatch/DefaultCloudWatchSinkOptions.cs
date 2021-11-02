using System;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace Synergy.Common.Logging.CloudWatch
{
    public class DefaultCloudWatchSinkOptions : CloudWatchSinkOptions
    {
        public DefaultCloudWatchSinkOptions(string logGroupName)
        {
            if (string.IsNullOrWhiteSpace(logGroupName))
            {
                throw new ArgumentNullException(nameof(logGroupName));
            }

            this.TextFormatter = new CloudWatchCompactJsonFormatter();
            this.CreateLogGroup = true;
            this.Period = TimeSpan.FromSeconds(30);
            this.LogGroupName = logGroupName;
        }
    }
}