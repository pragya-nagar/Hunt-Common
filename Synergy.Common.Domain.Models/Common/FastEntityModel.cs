using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Common.Domain.Models.Common
{
    public class FastEntityModel<T> : IResultModel
    {
        public T Id { get; set; }

        public string Name { get; set; }
    }
}
