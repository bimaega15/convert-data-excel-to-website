using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    /// <summary>
    /// Query baca untuk tabel master data turunan observasi. CRUD-nya berjalan lewat
    /// import Excel (bukan input manual), jadi service ini sengaja read-only.
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
