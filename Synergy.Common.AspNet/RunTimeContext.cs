using System;
using System.Diagnostics;
using System.Reflection;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public class RunTimeContext : IRunTimeContext
    {
        public string Version { get; } = Assembly.GetEntryAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        public TimeSpan Uptime => DateTime.Now - Process.GetCurrentProcess().StartTime;
    }
}
