using System;

namespace Synergy.Common.Abstracts
{
    public interface IRunTimeContext
    {
        string Version { get; }

        TimeSpan Uptime { get; }
    }
}
