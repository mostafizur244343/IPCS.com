using IPCS_Model.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IPCS_Service
{
    /// <summary>
    /// Applies Count + Skip/Take to a filtered IQueryable. The caller is
    /// responsible for building the filter and a stable OrderBy before calling.
    /// </summary>
    public static class PagingExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
