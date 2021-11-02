using System.Collections.Generic;
using Serilog.Events;

namespace Synergy.Common.Logging.Configuration
{
    public class SinksConfiguration
    {
        public CloudWatchConfiguration CloudWatch { get; set; }

        public ElasticSearchConfiguration ElasticSearch { get; set; }

        public LogEventLevel? MinimumLevel { get; set; }

        public Dictionary<string, LogEventLevel> LevelOverride { get; set; }
    }
}