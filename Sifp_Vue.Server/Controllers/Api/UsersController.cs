using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    [Route("api/users")]
    [Authorize(Roles = RoleNames.Administrator)]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters query, CancellationToken cancellationToken)
            => Success(await _service.GetPagedAsync(query, cancellationToken));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var user = await _service.GetByIdAsync(id, cancellationToken);
            return user is null
                ? Failure<UserDto>("User tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(user);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
            => Success(await _service.GetRolesAsync(cancellationToken));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<UserDto>();
            }

            var result = await _service.CreateAsync(request, CurrentUserName, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<UserDto>();
            }

            return FromResult(await _service.UpdateAsync(id, request, CurrentUserName, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            // Mencegah administrator terakhir mengunci dirinya sendiri di luar sistem.
            if (CurrentUserId == id)
            {
                return Failure<bool>("Anda tidak dapat menghapus akun yang sedang dipakai.");
            }

            return FromResult(await _service.DeleteAsync(id, cancellationToken), StatusCodes.Status404NotFound);
        }
    }
}
