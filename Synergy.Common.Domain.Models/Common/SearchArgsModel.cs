using System.Diagnostics.CodeAnalysis;

using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Common.Domain.Models.Common
{
    [SuppressMessage("Maintainability Rules", "SA1402", Justification = "Generic and non-generic definition of the same class")]
    public class SearchArgsModel<TFilter, TSortField> : SearchArgsModel<TSortField>
        where TSortField : struct
    {
        public TFilter Filter { get; set; }
    }

    [SuppressMessage("Maintainability Rules", "SA1402", Justification = "Generic and non-generic definition of the same class")]

    public class SearchArgsModel<TSortField> : SearchArgsModel
        where TSortField : struct
    {
        public TSortField? SortField { get; set; }

        public SortOrder? SortOrder { get; set; }
    }

    public class SearchArgsModel : IArgumentModel
    {
        public string FullSearch { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }
    }
}
