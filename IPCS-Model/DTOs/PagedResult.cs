using System;
using System.Collections.Generic;
using System.Linq;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// Server-side paging envelope. List endpoints return this shape only when
    /// the caller passes ?page= — without paging params the old full-list
    /// (array) shape is returned unchanged, so existing callers keep working.
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    public static class PagingDefaults
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public static int NormalizePage(int? page) => page.HasValue && page.Value > 0 ? page.Value : 1;

        public static int NormalizePageSize(int? pageSize)
        {
            if (!pageSize.HasValue || pageSize.Value <= 0) return DefaultPageSize;
            return Math.Min(pageSize.Value, MaxPageSize);
        }
    }
}
