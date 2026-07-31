using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    public interface IExcelImportService
    {
        /// <summary>
        /// Memproses satu workbook: menerapkan edit sel dari layar preview, memvalidasi
        /// sheet wajib, lalu mengganti seluruh master data dalam satu transaksi.
        /// </summary>
        Task<ApiResponse<ImportResultDto>> ImportAsync(
            Stream fileStream,
            string fileName,
            string? summaryJson,
            string? editsJson,
            string actor,
            CancellationToken cancellationToken = default);

        Task<PagedResult<ImportBatchDto>> GetBatchesAsync(QueryParameters query, CancellationToken cancellationToken = default);
        Task<ImportBatchDto?> GetBatchAsync(int id, CancellationToken cancellationToken = default);
    }
}
