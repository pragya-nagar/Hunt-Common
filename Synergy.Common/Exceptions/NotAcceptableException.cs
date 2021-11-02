using System;

namespace Synergy.Common.Exceptions
{
#pragma warning disable CA1058 // Types should not extend certain base types
    public class NotAcceptableException : ApplicationException
#pragma warning restore CA1058 // Types should not extend certain base types
    {
        public NotAcceptableException()
        {
        }

        public NotAcceptableException(string message)
            : base(message)
        {
        }

        public NotAcceptableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
