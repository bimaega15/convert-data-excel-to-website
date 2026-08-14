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

        /// <summary>Login manual (username/password) untuk klien Vue. Mengembalikan token bearer.</summary>
        Task<ApiResponse<LoginResultDto>> AuthenticateForApiAsync(LoginRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menukar identitas Windows (hasil Negotiate, mis. <c>DOMAIN\username</c>) dengan token bearer
        /// bila ada akun aplikasi yang username-nya cocok.
        /// </summary>
        Task<ApiResponse<LoginResultDto>> AuthenticateWindowsUserAsync(string windowsIdentityName, CancellationToken cancellationToken = default);
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
