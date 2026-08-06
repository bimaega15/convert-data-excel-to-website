using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ISifQuestionRepository _sifQuestions;
        private readonly IErrorTrapRepository _errorTraps;
        private readonly IHpToolRepository _hpTools;
        private readonly IDriftConditionRepository _driftConditions;
        private readonly ILatentConditionRepository _latentConditions;
        private readonly ICcvcLibraryRepository _ccvcLibrary;
        private readonly IObservationRepository _observations;
        private readonly IInitiativeRepository _initiatives;

        public MasterDataService(
            ISifQuestionRepository sifQuestions,
            IErrorTrapRepository errorTraps,
            IHpToolRepository hpTools,
            IDriftConditionRepository driftConditions,
            ILatentConditionRepository latentConditions,
            ICcvcLibraryRepository ccvcLibrary,
            IObservationRepository observations,
            IInitiativeRepository initiatives)
        {
            _sifQuestions = sifQuestions;
            _errorTraps = errorTraps;
            _hpTools = hpTools;
            _driftConditions = driftConditions;
            _latentConditions = latentConditions;
            _ccvcLibrary = ccvcLibrary;
            _observations = observations;
            _initiatives = initiatives;
        }

        public Task<PagedResult<SifQuestionDto>> GetSifQuestionsAsync(SifQuestionQuery query, CancellationToken cancellationToken = default)
            => _sifQuestions.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public Task<PagedResult<ErrorTrapDto>> GetErrorTrapsAsync(MasterDataQuery query, CancellationToken cancellationToken = default)
            => _errorTraps.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public Task<PagedResult<HpToolDto>> GetHpToolsAsync(MasterDataQuery query, CancellationToken cancellationToken = default)
            => _hpTools.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public Task<PagedResult<DriftConditionDto>> GetDriftConditionsAsync(MasterDataQuery query, CancellationToken cancellationToken = default)
            => _driftConditions.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public Task<PagedResult<LatentConditionDto>> GetLatentConditionsAsync(MasterDataQuery query, CancellationToken cancellationToken = default)
            => _latentConditions.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public Task<PagedResult<CcvcLibraryItemDto>> GetCcvcLibraryAsync(CcvcLibraryQuery query, CancellationToken cancellationToken = default)
            => _ccvcLibrary.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public async Task<CcvcLibraryItemDto?> GetCcvcItemAsync(string ccvcId, CancellationToken cancellationToken = default)
        {
            var item = await _ccvcLibrary.GetByCcvcIdAsync(ccvcId, cancellationToken);
            return item?.ToDto();
        }

        public async Task<ApiResponse<int>> DeleteAsync(string table, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0)
            {
                return ApiResponse<int>.Fail("Tidak ada baris yang dipilih untuk dihapus.");
            }

            // Tabel di sini semuanya "daun" (tanpa data turunan), jadi aman dihapus
            // langsung. Observasi & inisiatif punya endpoint tersendiri.
            Task<int>? deletion = table switch
            {
                "sif-questions" => _sifQuestions.DeleteByIdsAsync(ids, cancellationToken),
                "error-traps" => _errorTraps.DeleteByIdsAsync(ids, cancellationToken),
                "hp-tools" => _hpTools.DeleteByIdsAsync(ids, cancellationToken),
                "drift-conditions" => _driftConditions.DeleteByIdsAsync(ids, cancellationToken),
                "latent-conditions" => _latentConditions.DeleteByIdsAsync(ids, cancellationToken),
                "ccvc-library" => _ccvcLibrary.DeleteByIdsAsync(ids, cancellationToken),
                _ => null
            };

            if (deletion is null)
            {
                return ApiResponse<int>.Fail($"Tabel \"{table}\" tidak dikenal atau tidak mendukung hapus.");
            }

            var deleted = await deletion;
            return ApiResponse<int>.Ok(deleted, $"{deleted} baris dihapus.");
        }

        public async Task<IReadOnlyDictionary<string, int>> GetRowCountsAsync(CancellationToken cancellationToken = default)
        {
            return new Dictionary<string, int>
            {
                ["Observations"] = await _observations.CountAsync(cancellationToken),
                ["SifQuestions"] = await _sifQuestions.CountAsync(cancellationToken),
                ["ErrorTraps"] = await _errorTraps.CountAsync(cancellationToken),
                ["HpTools"] = await _hpTools.CountAsync(cancellationToken),
                ["DriftConditions"] = await _driftConditions.CountAsync(cancellationToken),
                ["LatentConditions"] = await _latentConditions.CountAsync(cancellationToken),
                ["CcvcLibraryItems"] = await _ccvcLibrary.CountAsync(cancellationToken),
                ["ImprovementInitiatives"] = await _initiatives.CountAsync(cancellationToken)
            };
        }
    }
}
