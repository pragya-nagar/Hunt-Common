using System;
using System.Collections.Generic;
using System.Text;

namespace Synergy.Common.Abstracts
{
    public interface IOperationContext
    {
        Guid CorrelationId { get; }

        Guid UserId { get; }

        string UserName { get; }
    }
}
