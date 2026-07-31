using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Helpers
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Menjalankan Count + Skip/Take dalam satu tempat sehingga semua endpoint list
        /// memakai bentuk paging yang sama.
        /// </summary>
        public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
            this IQueryable<TSource> query,
            QueryParameters parameters,
            Func<TSource, TResult> projection,
            CancellationToken cancellationToken = default)
        {
            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<TResult>
            {
                Items = items.Select(projection).ToList(),
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalItems = total
            };
        }

        /// <summary>Menerapkan predikat hanya bila kondisinya terpenuhi, agar rantai query tetap terbaca.</summary>
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
    }
}
