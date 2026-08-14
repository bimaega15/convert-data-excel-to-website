using System.Security.Claims;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    /// <summary>
    /// Login area admin (/admin, cookie) dan login klien Vue (/api/auth, token bearer).
    /// </summary>
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Username atau password salah.";
        private const string WindowsAccountNotFoundMessage =
            "Akun Windows ini belum terdaftar di aplikasi SIFP Assurance. Gunakan login manual atau hubungi admin.";

        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserClaimsFactory _claimsFactory;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            IPasswordHasher passwordHasher,
            IUserClaimsFactory claimsFactory,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _claimsFactory = claimsFactory;
            _jwtTokenService = jwtTokenService;
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

        public async Task<ApiResponse<LoginResultDto>> AuthenticateForApiAsync(
            LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request, cancellationToken);
            if (user is null)
            {
                return ApiResponse<LoginResultDto>.Fail(InvalidCredentialsMessage);
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login API berhasil untuk {Username}", user.Username);
            return ApiResponse<LoginResultDto>.Ok(IssueToken(user, roles, request.RememberMe), "Login berhasil.");
        }

        public async Task<ApiResponse<LoginResultDto>> AuthenticateWindowsUserAsync(
            string windowsIdentityName, CancellationToken cancellationToken = default)
        {
            // Identitas Negotiate datang berformat "DOMAIN\username"; hanya bagian
            // username yang dicocokkan karena tabel User belum punya kolom domain terpisah.
            var separatorIndex = windowsIdentityName.IndexOf('\\');
            var username = separatorIndex >= 0 ? windowsIdentityName[(separatorIndex + 1)..] : windowsIdentityName;

            var user = await _users.GetByUsernameWithRolesAsync(username.Trim(), cancellationToken);
            if (user is null || !user.IsActive)
            {
                _logger.LogWarning("Login Windows ditolak: {WindowsIdentity} tidak cocok dengan akun aktif manapun", windowsIdentityName);
                return ApiResponse<LoginResultDto>.Fail(WindowsAccountNotFoundMessage);
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login Windows berhasil untuk {Username} ({WindowsIdentity})", user.Username, windowsIdentityName);
            return ApiResponse<LoginResultDto>.Ok(IssueToken(user, roles), "Login berhasil.");
        }

        private LoginResultDto IssueToken(Models.Entities.User user, List<string> roles, bool rememberMe = false)
        {
            var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user, roles, rememberMe);
            return new LoginResultDto { Token = token, ExpiresAtUtc = expiresAtUtc, User = user.ToDto() };
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
