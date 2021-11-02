using System.Threading;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public sealed class OperationContextStore : IOperationContextFactory, IOperationContextAccessor
    {
        private static readonly AsyncLocal<IOperationContext> OperationContext = new AsyncLocal<IOperationContext>();

        public IOperationContext Current => OperationContext.Value;

        public void Create(IOperationContext context)
        {
            OperationContext.Value = context;
        }

        public void Dispose()
        {
            OperationContext.Value = null;
        }
    }
}