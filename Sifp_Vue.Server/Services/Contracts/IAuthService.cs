using System.Security.Claims;
using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    public interface IAuthService
    {
        /// <summary>
        /// Memvalidasi kredensial untuk login cookie area admin.
        /// Mengembalikan identity siap sign-in bila user berhak membuka /admin.
        /// </summary>
        Task<ApiResponse<ClaimsIdentity>> AuthenticateForAdminAsync(LoginRequest request, string authenticationScheme, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login manual (username/password) untuk klien Vue. Tidak langsung membalas token
        /// bearer — mengembalikan tantangan MFA yang harus diselesaikan lewat
        /// <see cref="VerifyMfaAsync"/> sebelum sesi benar-benar aktif.
        /// </summary>
        Task<ApiResponse<LoginChallengeDto>> AuthenticateForApiAsync(LoginRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menukar email hasil login eksternal (Microsoft Entra ID) dengan tantangan MFA
        /// (sama seperti <see cref="AuthenticateForApiAsync"/>, token bearer baru terbit
        /// setelah <see cref="VerifyMfaAsync"/>). Email wajib berada di domain yang
        /// diizinkan (mis. @pertamina.com). Bila Auth:AutoProvision aktif, akun yang belum
        /// ada dibuat otomatis dengan role default; bila tidak, hanya akun terdaftar dan
        /// aktif yang diterima.
        /// </summary>
        Task<ApiResponse<LoginChallengeDto>> AuthenticateExternalEmailAsync(string email, string? displayName = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login via Windows Authenticator (NTLM/Kerberos Windows Principal).
        /// Memvalidasi atau meng-autoprovision akun Windows lokal/domain dan membalas tantangan MFA.
        /// </summary>
        Task<ApiResponse<LoginChallengeDto>> AuthenticateWindowsUserAsync(string winIdentityName, string? displayName = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Langkah kedua login: memverifikasi kode 6 digit terhadap tantangan yang
        /// diterbitkan oleh <see cref="AuthenticateForApiAsync"/>/<see cref="AuthenticateExternalEmailAsync"/>.
        /// Bila tantangan itu bertipe setup, secret yang tertanam di token baru disimpan
        /// dan MFA diaktifkan setelah kode ini terbukti cocok. Sukses -> token bearer asli.
        /// </summary>
        Task<ApiResponse<LoginResultDto>> VerifyMfaAsync(MfaVerifyRequest request, CancellationToken cancellationToken = default);
    }

    public interface IUserService
    {
        Task<PagedResult<UserDto>> GetPagedAsync(QueryParameters query, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, string actor, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserRequest request, string actor, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
