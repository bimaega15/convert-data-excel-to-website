using System.Security.Claims;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Username atau password salah.";

        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenHandler _tokenHandler;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            IPasswordHasher passwordHasher,
            IJwtTokenHandler tokenHandler,
            ILogger<AuthService> logger)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _tokenHandler = tokenHandler;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request, cancellationToken);
            if (user is null)
            {
                return ApiResponse<LoginResponse>.Fail(InvalidCredentialsMessage);
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();
            var (token, expiresAt) = _tokenHandler.CreateToken(user, roles);

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login API berhasil untuk {Username}", user.Username);

            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                Token = token,
                ExpiresAtUtc = expiresAt,
                User = user.ToDto()
            }, "Login berhasil.");
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

            var identity = _tokenHandler.BuildIdentity(user, roles.Select(r => r.Name), authenticationScheme);
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

        public async Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetWithRolesAsync(userId, cancellationToken);
            return user?.ToDto();
        }
    }
}
