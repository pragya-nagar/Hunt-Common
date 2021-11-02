using System.Linq;

namespace Synergy.Common.DAL.Abstract
{
    public interface IQueryProvider<out T>
        where T : class, IEntity
    {
        IQueryable<T> Query { get; }
    }
}
