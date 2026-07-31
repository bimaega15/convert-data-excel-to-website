using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Repositories
{
    public interface IWorksheetRepository : IRepository<Worksheet>
    {
        /// <summary>Batch import terakhir yang berhasil; null bila belum pernah ada import sukses.</summary>
        Task<ImportBatch?> GetLatestCompletedBatchAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Worksheet>> GetByBatchAsync(int batchId, CancellationToken cancellationToken = default);
        Task<Worksheet?> GetBySlugAsync(int batchId, string slug, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorksheetRow>> GetRowsAsync(int worksheetId, CancellationToken cancellationToken = default);
    }

    public class WorksheetRepository : Repository<Worksheet>, IWorksheetRepository
    {
        public WorksheetRepository(SifpDbContext context) : base(context)
        {
        }

        public Task<ImportBatch?> GetLatestCompletedBatchAsync(CancellationToken cancellationToken = default)
            => Context.ImportBatches
                .AsNoTracking()
                .Where(x => x.Status == ImportStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<Worksheet>> GetByBatchAsync(int batchId, CancellationToken cancellationToken = default)
            => await Query()
                .Where(x => x.ImportBatchId == batchId)
                .OrderBy(x => x.SheetIndex)
                .ToListAsync(cancellationToken);

        public Task<Worksheet?> GetBySlugAsync(int batchId, string slug, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.ImportBatchId == batchId && x.Slug == slug, cancellationToken);

        public async Task<IReadOnlyList<WorksheetRow>> GetRowsAsync(int worksheetId, CancellationToken cancellationToken = default)
            => await Context.WorksheetRows
                .AsNoTracking()
                .Where(x => x.WorksheetId == worksheetId)
                .OrderBy(x => x.RowIndex)
                .ToListAsync(cancellationToken);
    }

    public interface IImportBatchRepository : IRepository<ImportBatch>
    {
        Task<IReadOnlyList<ImportBatch>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
        Task<ImportBatch?> GetLatestCompletedAsync(CancellationToken cancellationToken = default);
    }

    public class ImportBatchRepository : Repository<ImportBatch>, IImportBatchRepository
    {
        public ImportBatchRepository(SifpDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ImportBatch>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
            => await Query()
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

        public Task<ImportBatch?> GetLatestCompletedAsync(CancellationToken cancellationToken = default)
            => Query()
                .Where(x => x.Status == ImportStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
