using System.Linq;

using Synergy.Common.DAL.Abstract;

namespace Synergy.Common.DAL.Access.PostgreSQL
{
    public class QueryProvider<T> : IQueryProvider<T>
        where T : class, IEntity
    {
        public QueryProvider(IDataAccess dataAccess)
        {
            this.DataAccess = dataAccess;
        }

        public virtual IQueryable<T> Query => this.DataAccess.GetQueryable<T>();

        protected IDataAccess DataAccess { get; }
    }
}
