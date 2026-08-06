using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;

namespace Sifp_Vue.Server.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SifpDbContext Context;
        protected readonly DbSet<T> Set;

        public Repository(SifpDbContext context)
        {
            Context = context;
            Set = context.Set<T>();
        }

        public virtual IQueryable<T> Query() => Set.AsNoTracking();

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await Set.FindAsync(new object?[] { id }, cancellationToken);

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await Set.AsNoTracking().ToListAsync(cancellationToken);

        public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Set.CountAsync(cancellationToken);

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Set.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            => await Set.AddRangeAsync(entities, cancellationToken);

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            Set.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await Set.FindAsync(new object?[] { id }, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            Set.Remove(entity);
            return true;
        }

        // ExecuteDelete: satu perintah DELETE ... WHERE Id IN (...) di server, tanpa
        // memuat baris ke memori. EF.Property dipakai karena Id dideklarasikan di tiap
        // entitas konkret, bukan di kelas generik T.
        public virtual Task<int> DeleteByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0)
            {
                return Task.FromResult(0);
            }

            return Set.Where(e => ids.Contains(EF.Property<int>(e, "Id"))).ExecuteDeleteAsync(cancellationToken);
        }

        // ExecuteDelete: satu perintah DELETE di server, tanpa memuat baris ke memori.
        public virtual Task<int> DeleteAllAsync(CancellationToken cancellationToken = default)
            => Set.ExecuteDeleteAsync(cancellationToken);

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Context.SaveChangesAsync(cancellationToken);
    }
}
