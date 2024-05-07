using System.Collections.Generic;

namespace Melbeez.Common.Models
{
    public class PagedListCriteria
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public string SearchText { get; set; }
        public IList<string>? OrderBy { get; set; }
    }
}
