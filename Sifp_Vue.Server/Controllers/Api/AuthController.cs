using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>Autentikasi untuk klien Vue: menerbitkan dan memeriksa JWT.</summary>
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>POST /api/auth/login — menukar username &amp; password dengan access token.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<LoginResponse>();
            }

            var result = await _authService.LoginAsync(request, cancellationToken);
            return FromResult(result, StatusCodes.Status401Unauthorized);
        }

        /// <summary>GET /api/auth/me — profil user dari token yang sedang dipakai.</summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var userId = CurrentUserId;
            if (userId is null)
            {
                return Failure<UserDto>("Token tidak memuat identitas user.", StatusCodes.Status401Unauthorized);
            }

            var user = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);
            return user is null
                ? Failure<UserDto>("User tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(user);
        }
    }
}
