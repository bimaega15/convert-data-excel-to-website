namespace Sifp_Vue.Server.Repositories
{
    /// <summary>
    /// Operasi CRUD generik. Repository spesifik mewarisi ini dan hanya menambahkan
    /// query khusus domainnya, sehingga kode berulang tidak disalin per entitas.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Query tanpa tracking untuk kebutuhan baca. Mengembalikan IQueryable supaya
        /// filter dan paging tetap dieksekusi di sisi database.
        /// </summary>
        IQueryable<T> Query();

        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Menghapus seluruh baris tabel — dipakai saat import mengganti data lama.</summary>
        Task<int> DeleteAllAsync(CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
