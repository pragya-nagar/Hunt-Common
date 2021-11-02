using System;

namespace Synergy.Common.Exceptions
{
#pragma warning disable CA1058 // Types should not extend certain base types
    public class NotFoundException : ApplicationException
#pragma warning restore CA1058 // Types should not extend certain base types
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
