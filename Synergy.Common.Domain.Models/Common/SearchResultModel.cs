using System.Collections.Generic;

using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Common.Domain.Models.Common
{
    public class SearchResultModel<T>
        where T : IResultModel
    {
        public int TotalCount { get; set; }

        public IEnumerable<T> List { get; set; }
    }
}
