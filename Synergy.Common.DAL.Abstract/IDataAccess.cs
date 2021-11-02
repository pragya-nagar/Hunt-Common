using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.Common.DAL.Abstract
{
    public interface IDataAccess : IDisposable
    {
        IQueryable<T> GetQueryable<T>()
            where T : class, IEntity;

        int Execute(string query, params object[] parameters);

        Task<int> ExecuteAsync(string query, CancellationToken cancellationToken = default(CancellationToken), params object[] parameters);
    }
}
