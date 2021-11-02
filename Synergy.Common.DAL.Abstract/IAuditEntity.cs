using System;

namespace Synergy.Common.DAL.Abstract
{
    public interface IAuditEntity : IEntity
    {
        DateTime CreatedOn { get; set; }

        Guid CreatedById { get; set; }

        DateTime ModifiedOn { get; set; }

        Guid ModifiedById { get; set; }

        DateTime? DeletedOn { get; set; }
    }

    public interface IAuditEntity<T> : IAuditEntity
    {
        T Id { get; set; }
    }
}
