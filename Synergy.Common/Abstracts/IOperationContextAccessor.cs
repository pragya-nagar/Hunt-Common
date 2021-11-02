namespace Synergy.Common.Abstracts
{
    public interface IOperationContextAccessor
    {
        IOperationContext Current { get; }
    }
}