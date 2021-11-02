using System;

namespace Synergy.Common.Abstracts
{
    public interface IClockService
    {
        DateTime Now { get; }

        DateTime UtcNow { get; }
    }
}
