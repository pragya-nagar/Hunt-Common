using System;

namespace Synergy.Common.Abstracts
{
    public interface IOperationContextFactory : IDisposable
    {
        void Create(IOperationContext context);
    }
}