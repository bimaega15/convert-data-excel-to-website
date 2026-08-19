using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    /// <summary>
    /// Query baca untuk tabel master data turunan observasi, plus penghapusan baris
    /// (single/multiple/all) yang dipicu dari tabel di UI. Penambahan data tetap
    /// lewat import Excel.
    /// </summary>
    public interface IMasterDataService
    {
        Task<PagedResult<SifQuestionDto>> GetSifQuestionsAsync(SifQuestionQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<ErrorTrapDto>> GetErrorTrapsAsync(MasterDataQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<HpToolDto>> GetHpToolsAsync(MasterDataQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<DriftConditionDto>> GetDriftConditionsAsync(MasterDataQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<LatentConditionDto>> GetLatentConditionsAsync(MasterDataQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<CcvcLibraryItemDto>> GetCcvcLibraryAsync(CcvcLibraryQuery query, CancellationToken cancellationToken = default);

        Task<CcvcLibraryItemDto?> GetCcvcItemAsync(string ccvcId, CancellationToken cancellationToken = default);

        /// <summary>Jumlah baris per tabel, dipakai kartu ringkasan di halaman admin.</summary>
        Task<IReadOnlyDictionary<string, int>> GetRowCountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Menghapus baris pada salah satu tabel master (nama tabel = segmen rute,
        /// mis. "error-traps"). Menghapus banyak Id sekaligus; "hapus semua" cukup
        /// mengirim seluruh Id yang ada. Mengembalikan jumlah baris yang terhapus.
        /// </summary>
        Task<ApiResponse<int>> DeleteAsync(string table, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

        /// <summary>Menambahkan satu baris data baru ke tabel master yang ditentukan.</summary>
        Task<ApiResponse<object>> CreateRowAsync(string table, System.Text.Json.JsonElement body, CancellationToken cancellationToken = default);
    }

    public interface IInitiativeService
    {
        Task<PagedResult<InitiativeDto>> GetPagedAsync(InitiativeQuery query, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InitiativeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<InitiativeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<InitiativeDto>> CreateAsync(InitiativeRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<InitiativeDto>> UpdateAsync(int id, InitiativeRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
