using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    public interface IObservationService
    {
        Task<PagedResult<ObservationDto>> GetPagedAsync(ObservationQuery query, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ObservationDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ObservationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ObservationDto?> GetByCodeAsync(string obsCode, CancellationToken cancellationToken = default);

        /// <summary>Detail satu observasi lengkap dengan seluruh data anaknya.</summary>
        Task<ObservationDetailDto?> GetDetailAsync(int id, CancellationToken cancellationToken = default);

        Task<ApiResponse<ObservationDto>> CreateAsync(ObservationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<ObservationDto>> UpdateAsync(int id, ObservationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Nilai unik untuk mengisi dropdown filter di UI.</summary>
        Task<ObservationFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default);
    }

    public class ObservationDetailDto
    {
        public ObservationDto Observation { get; set; } = new();
        public List<SifQuestionDto> SifQuestions { get; set; } = new();
        public List<ErrorTrapDto> ErrorTraps { get; set; } = new();
        public List<HpToolDto> HpTools { get; set; } = new();
        public List<DriftConditionDto> DriftConditions { get; set; } = new();
        public List<LatentConditionDto> LatentConditions { get; set; } = new();
    }

    public class ObservationFilterOptionsDto
    {
        public List<int> Zonas { get; set; } = new();
        public List<string> ProtocolCodes { get; set; } = new();
        public List<string> Sites { get; set; } = new();
        public List<string> Companies { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
    }
}
