using System;

using Synergy.Common.Abstracts;

namespace Synergy.Common
{
    public class ClockService : IClockService
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
