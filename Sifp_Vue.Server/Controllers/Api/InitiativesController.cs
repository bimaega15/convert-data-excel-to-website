using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    [Route("api/initiatives")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InitiativesController : ApiControllerBase
    {
        private readonly IInitiativeService _service;

        public InitiativesController(IInitiativeService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InitiativeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] InitiativeQuery query, CancellationToken cancellationToken)
            => Success(await _service.GetPagedAsync(query, cancellationToken));

        /// <summary>GET /api/initiatives/all — bentuknya sama dengan <c>initiatives.json</c>.</summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
            => Success(await _service.GetAllAsync(cancellationToken));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _service.GetByIdAsync(id, cancellationToken);
            return item is null
                ? Failure<InitiativeDto>("Inisiatif tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InitiativeRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<InitiativeDto>();
            }

            var result = await _service.CreateAsync(request, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Key }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] InitiativeRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<InitiativeDto>();
            }

            return FromResult(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
            => FromResult(await _service.DeleteAsync(id, cancellationToken), StatusCodes.Status404NotFound);
    }
}
