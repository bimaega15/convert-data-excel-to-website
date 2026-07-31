using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>CRUD observasi. Baca terbuka untuk semua user login; tulis khusus Administrator/Verifier.</summary>
    [Route("api/observations")]
    public class ObservationsController : ApiControllerBase
    {
        private const string WriteRoles = RoleNames.Administrator + "," + RoleNames.Verifier;

        private readonly IObservationService _service;

        public ObservationsController(IObservationService service)
        {
            _service = service;
        }

        /// <summary>GET /api/observations — daftar berhalaman dengan filter dan pencarian.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ObservationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] ObservationQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetPagedAsync(query, cancellationToken));

        /// <summary>
        /// GET /api/observations/all — seluruh baris tanpa paging, bentuknya sama dengan
        /// <c>observations.json</c> supaya halaman master Vue bisa memakainya langsung.
        /// </summary>
        [HttpGet("all")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ObservationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
            => Success(await _service.GetAllAsync(cancellationToken));

        /// <summary>GET /api/observations/filter-options — nilai unik untuk dropdown filter.</summary>
        [HttpGet("filter-options")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
            => Success(await _service.GetFilterOptionsAsync(cancellationToken));

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ObservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ObservationDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _service.GetByIdAsync(id, cancellationToken);
            return item is null
                ? Failure<ObservationDto>("Observasi tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(item);
        }

        /// <summary>GET /api/observations/code/{obsCode} — pencarian berdasarkan Obs_ID, mis. OBS-001.</summary>
        [HttpGet("code/{obsCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCode(string obsCode, CancellationToken cancellationToken)
        {
            var item = await _service.GetByCodeAsync(obsCode, cancellationToken);
            return item is null
                ? Failure<ObservationDto>($"Observasi \"{obsCode}\" tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(item);
        }

        /// <summary>GET /api/observations/{id}/detail — observasi beserta seluruh data turunannya.</summary>
        [HttpGet("{id:int}/detail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetail(int id, CancellationToken cancellationToken)
        {
            var detail = await _service.GetDetailAsync(id, cancellationToken);
            return detail is null
                ? Failure<ObservationDetailDto>("Observasi tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(detail);
        }

        [HttpPost]
        [Authorize(Roles = WriteRoles)]
        [ProducesResponseType(typeof(ApiResponse<ObservationDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ObservationRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<ObservationDto>();
            }

            var result = await _service.CreateAsync(request, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Key }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = WriteRoles)]
        public async Task<IActionResult> Update(int id, [FromBody] ObservationRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<ObservationDto>();
            }

            return FromResult(await _service.UpdateAsync(id, request, cancellationToken));
        }

        /// <summary>DELETE /api/observations/{id} — menghapus observasi beserta seluruh data turunannya.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
            => FromResult(await _service.DeleteAsync(id, cancellationToken), StatusCodes.Status404NotFound);
    }
}
