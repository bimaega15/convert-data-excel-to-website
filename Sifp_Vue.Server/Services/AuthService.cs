using System.Security.Claims;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Services
{
    /// <summary>
    /// Login area admin (/admin). Klien Vue tidak memakai service ini: endpoint /api
    /// terbuka dan pembatasan aksesnya direncanakan lewat Windows Authentication di IIS.
    /// </summary>
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Username atau password salah.";

        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserClaimsFactory _claimsFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            IPasswordHasher passwordHasher,
            IUserClaimsFactory claimsFactory,
            ILogger<AuthService> logger)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _claimsFactory = claimsFactory;
            _logger = logger;
        }

        public async Task<ApiResponse<ClaimsIdentity>> AuthenticateForAdminAsync(
            LoginRequest request,
            string authenticationScheme,
            CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request, cancellationToken);
            if (user is null)
            {
                return ApiResponse<ClaimsIdentity>.Fail(InvalidCredentialsMessage);
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!).ToList();
            if (!roles.Any(r => r.CanAccessAdmin))
            {
                _logger.LogWarning("User {Username} mencoba masuk area admin tanpa role yang berwenang", user.Username);
                return ApiResponse<ClaimsIdentity>.Fail("Akun Anda tidak memiliki akses ke area admin.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            var identity = _claimsFactory.BuildIdentity(user, roles.Select(r => r.Name), authenticationScheme);
            _logger.LogInformation("Login admin berhasil untuk {Username}", user.Username);

            return ApiResponse<ClaimsIdentity>.Ok(identity, "Login berhasil.");
        }

        /// <summary>
        /// Verifikasi hash tetap dijalankan walau user tidak ada, supaya waktu respons
        /// tidak membedakan "username salah" dari "password salah".
        /// </summary>
        private async Task<Models.Entities.User?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByUsernameWithRolesAsync(request.Username.Trim(), cancellationToken);

            if (user is null)
            {
                _passwordHasher.Verify(request.Password, DummyHash);
                return null;
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Password salah untuk {Username}", request.Username);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login ditolak: akun {Username} non-aktif", request.Username);
                return null;
            }

            return user;
        }

        // Hash bernilai tetap untuk membakar waktu verifikasi saat user tidak ditemukan.
        private const string DummyHash =
            "210000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
    }
}
