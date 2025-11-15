using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public PagedResult(List<T> items, int totalItems, int pageSize, int currentPage)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }
}
