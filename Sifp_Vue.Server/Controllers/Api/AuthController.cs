using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// Login klien Vue: token bearer lewat username/password atau Windows SSO (Negotiate).
    /// Sengaja tidak memakai [AllowAnonymous] di level controller supaya skema auth
    /// per-action (Negotiate untuk /windows, JwtBearer untuk /me) benar-benar ditegakkan.
    /// </summary>
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        /// <summary>POST /api/auth/login — login manual username/password, mengembalikan token bearer.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationFailure<LoginResultDto>();
            }

            return FromResult(await _authService.AuthenticateForApiAsync(request, cancellationToken), StatusCodes.Status401Unauthorized);
        }

        /// <summary>
        /// GET /api/auth/windows — login SSO. Browser menegosiasikan identitas Windows
        /// yang sedang login lewat NTLM/Kerberos sebelum action ini dijalankan.
        /// </summary>
        [HttpGet("windows")]
        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Windows(CancellationToken cancellationToken)
        {
            var windowsIdentityName = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(windowsIdentityName))
            {
                return Failure<LoginResultDto>("Identitas Windows tidak terbaca dari browser.", StatusCodes.Status401Unauthorized);
            }

            return FromResult(await _authService.AuthenticateWindowsUserAsync(windowsIdentityName, cancellationToken), StatusCodes.Status401Unauthorized);
        }

        /// <summary>GET /api/auth/me — data user pemilik token bearer yang sedang dipakai.</summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            if (CurrentUserId is not { } id)
            {
                return Failure<UserDto>("Token tidak valid.", StatusCodes.Status401Unauthorized);
            }

            var user = await _userService.GetByIdAsync(id, cancellationToken);
            return user is null
                ? Failure<UserDto>("User tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(user);
        }
    }
}
