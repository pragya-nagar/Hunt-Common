using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

using Synergy.Common.DAL.Abstract;

namespace Synergy.Common.DAL.Access.PostgreSQL
{
    public abstract class BaseDataAccess : DbContext, IDataAccess
    {
        protected BaseDataAccess(ILoggerFactory loggerFactory, string nameOrConnectionString)
        {
            this.LoggerFactory = loggerFactory;
            this.Connection = new NpgsqlConnection(nameOrConnectionString);
        }

        protected ILoggerFactory LoggerFactory { get; }

        protected IDbConnection Connection { get; }

        public IQueryable<T> GetQueryable<T>()
            where T : class, IEntity => this.Set<T>().AsNoTracking();

        public int Execute(string query, params object[] parameters) => this.Database.ExecuteSqlCommand(query, parameters);

        public async Task<int> ExecuteAsync(string query, CancellationToken cancellationToken = default(CancellationToken), params object[] parameters) => await Database.ExecuteSqlCommandAsync(query, cancellationToken, parameters).ConfigureAwait(false);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(this.LoggerFactory);
            optionsBuilder.UseNpgsql((DbConnection)this.Connection);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}