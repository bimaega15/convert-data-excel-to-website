using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// Basis seluruh endpoint /api.
    ///
    /// Endpoint /api terbuka tanpa login: klien Vue tidak punya halaman login sendiri
    /// karena pembatasan akses direncanakan lewat Windows Authentication di IIS
    /// perusahaan. Area /admin tetap memakai cookie login sendiri.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Nama user untuk kolom audit. Saat Windows Authentication aktif, nilainya
        /// terisi otomatis dari akun domain; sebelum itu jatuh ke "SYSTEM".
        /// </summary>
        protected string CurrentUserName => User.Identity?.Name ?? "SYSTEM";

        /// <summary>Id user; hanya terisi pada endpoint yang masih memakai cookie admin.</summary>
        protected int? CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        protected IActionResult Success<T>(T data, string message = "OK")
            => Ok(ApiResponse<T>.Ok(data, message));

        protected IActionResult Failure<T>(string message, int statusCode = StatusCodes.Status400BadRequest)
            => StatusCode(statusCode, ApiResponse<T>.Fail(message));

        /// <summary>Menerjemahkan hasil service menjadi status HTTP yang sesuai.</summary>
        protected IActionResult FromResult<T>(ApiResponse<T> result, int failureStatusCode = StatusCodes.Status400BadRequest)
            => result.Status == ApiStatus.Success ? Ok(result) : StatusCode(failureStatusCode, result);

        /// <summary>Mengubah ModelState yang tidak valid menjadi amplop respons standar.</summary>
        protected IActionResult ValidationFailure<T>()
        {
            var errors = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(ApiResponse<T>.Fail("Data yang dikirim tidak valid.", errors));
        }
    }
}
