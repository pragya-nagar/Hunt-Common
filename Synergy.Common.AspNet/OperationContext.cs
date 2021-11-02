using System;
using System.Threading;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public class OperationContext : IOperationContext
    {
        public OperationContext(Guid correlationId, Guid userId, string userName)
        {
            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(correlationId)} should not be empty.", nameof(correlationId));
            }

            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(userId)} should not be empty.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException($"{nameof(userName)} should not be empty.", nameof(userName));
            }

            this.CorrelationId = correlationId;
            this.UserId = userId;
            this.UserName = userName;
        }

        public Guid CorrelationId { get; }

        public Guid UserId { get; }

        public string UserName { get; }
    }
}
