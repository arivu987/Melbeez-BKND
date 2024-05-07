using System.Collections.Generic;
using System.Linq;

namespace Melbeez.Common.Models
{
    public class PagedList<T>
    {
        /// <summary>
        /// Empty constructor was initially created for use in API tests.
        /// Double check before using this constructor elsewhere.
        /// </summary>
        public PagedList()
        {
            Data = new List<T>();
        }

        public PagedList(List<T> data, int totalCount)
        {
            Data = data.ToList();
            TotalCount = totalCount;
        }

        public List<T> Data { get; set; }

        public int TotalCount { get; set; }
    }
}
