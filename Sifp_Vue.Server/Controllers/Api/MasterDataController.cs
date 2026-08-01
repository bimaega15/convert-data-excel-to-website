using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// Endpoint baca untuk tabel master data turunan observasi. Satu controller
    /// dipakai bersama karena bentuk query dan responsnya seragam.
    /// </summary>
    [Route("api/master")]
    public class MasterDataController : ApiControllerBase
    {
        private readonly IMasterDataService _service;

        public MasterDataController(IMasterDataService service)
        {
            _service = service;
        }

        /// <summary>GET /api/master/sif-questions</summary>
        [HttpGet("sif-questions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SifQuestionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SifQuestions([FromQuery] SifQuestionQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetSifQuestionsAsync(query, cancellationToken));

        /// <summary>GET /api/master/error-traps</summary>
        [HttpGet("error-traps")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ErrorTrapDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ErrorTraps([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetErrorTrapsAsync(query, cancellationToken));

        /// <summary>GET /api/master/hp-tools</summary>
        [HttpGet("hp-tools")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HpToolDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> HpTools([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetHpToolsAsync(query, cancellationToken));

        /// <summary>GET /api/master/drift-conditions</summary>
        [HttpGet("drift-conditions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DriftConditionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DriftConditions([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetDriftConditionsAsync(query, cancellationToken));

        /// <summary>GET /api/master/latent-conditions</summary>
        [HttpGet("latent-conditions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LatentConditionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> LatentConditions([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetLatentConditionsAsync(query, cancellationToken));

        /// <summary>GET /api/master/ccvc-library</summary>
        [HttpGet("ccvc-library")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CcvcLibraryItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CcvcLibrary([FromQuery] CcvcLibraryQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetCcvcLibraryAsync(query, cancellationToken));

        /// <summary>GET /api/master/ccvc-library/{ccvcId} — satu entri library, mis. CLSR01-A.</summary>
        [HttpGet("ccvc-library/{ccvcId}")]
        public async Task<IActionResult> CcvcItem(string ccvcId, CancellationToken cancellationToken)
        {
            var item = await _service.GetCcvcItemAsync(ccvcId, cancellationToken);
            return item is null
                ? Failure<CcvcLibraryItemDto>($"CCVC \"{ccvcId}\" tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(item);
        }

        /// <summary>GET /api/master/counts — jumlah baris per tabel master data.</summary>
        [HttpGet("counts")]
        public async Task<IActionResult> Counts(CancellationToken cancellationToken)
            => Success(await _service.GetRowCountsAsync(cancellationToken));
    }
}
