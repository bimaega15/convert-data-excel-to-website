using System.Security.Claims;
using Microsoft.Extensions.Options;
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
        private const string ExternalAccountNotFoundMessage =
            "Akun Microsoft ini belum terdaftar di aplikasi SIFP Assurance. Hubungi admin untuk didaftarkan.";

        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserClaimsFactory _claimsFactory;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            IRoleRepository roles,
            IPasswordHasher passwordHasher,
            IUserClaimsFactory claimsFactory,
            IJwtTokenService jwtTokenService,
            IOptions<AuthOptions> authOptions,
            ILogger<AuthService> logger)
        {
            _users = users;
            _roles = roles;
            _passwordHasher = passwordHasher;
            _claimsFactory = claimsFactory;
            _jwtTokenService = jwtTokenService;
            _authOptions = authOptions.Value;
            _logger = logger;
        }

        /// <summary>Pesan penolakan domain email, mencantumkan domain yang diizinkan.</summary>
        private string EmailDomainRejectedMessage()
        {
            var domains = string.Join(", ", _authOptions.EffectiveDomains.Select(d => "@" + d.Trim()));
            return $"Login ditolak. Hanya akun dengan email domain {domains} yang diizinkan masuk.";
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

            if (!_authOptions.IsEmailDomainAllowed(user.Email))
            {
                _logger.LogWarning("Login admin ditolak: email {Email} di luar domain yang diizinkan", user.Email);
                return ApiResponse<ClaimsIdentity>.Fail(EmailDomainRejectedMessage());
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

            if (!_authOptions.IsEmailDomainAllowed(user.Email))
            {
                _logger.LogWarning("Login API ditolak: email {Email} di luar domain yang diizinkan", user.Email);
                return ApiResponse<LoginResultDto>.Fail(EmailDomainRejectedMessage());
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login API berhasil untuk {Username}", user.Username);
            return ApiResponse<LoginResultDto>.Ok(IssueToken(user, roles, request.RememberMe), "Login berhasil.");
        }

        public async Task<ApiResponse<LoginResultDto>> AuthenticateExternalEmailAsync(
            string email, string? displayName = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ApiResponse<LoginResultDto>.Fail("Email tidak terbaca dari akun Microsoft.");
            }

            email = email.Trim();

            // Batasan domain ditegakkan lebih dulu supaya email di luar @pertamina.com
            // ditolak dengan pesan yang jelas, bukan "belum terdaftar".
            if (!_authOptions.IsEmailDomainAllowed(email))
            {
                _logger.LogWarning("Login Microsoft ditolak: email {Email} di luar domain yang diizinkan", email);
                return ApiResponse<LoginResultDto>.Fail(EmailDomainRejectedMessage());
            }

            var user = await _users.GetByEmailWithRolesAsync(email, cancellationToken);

            if (user is null)
            {
                if (!_authOptions.AutoProvision)
                {
                    _logger.LogWarning("Login Microsoft ditolak: email {Email} belum terdaftar (AutoProvision nonaktif)", email);
                    return ApiResponse<LoginResultDto>.Fail(ExternalAccountNotFoundMessage);
                }

                var provisioned = await ProvisionExternalUserAsync(email, displayName, cancellationToken);
                if (provisioned is null)
                {
                    return ApiResponse<LoginResultDto>.Fail(
                        $"Role default '{_authOptions.AutoProvisionRole}' tidak ditemukan. Hubungi admin.");
                }
                user = provisioned;
            }
            else if (!user.IsActive)
            {
                _logger.LogWarning("Login Microsoft ditolak: akun {Email} non-aktif", email);
                return ApiResponse<LoginResultDto>.Fail("Akun Anda dinonaktifkan. Hubungi admin.");
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login Microsoft berhasil untuk {Username} ({Email})", user.Username, email);
            return ApiResponse<LoginResultDto>.Ok(IssueToken(user, roles), "Login berhasil.");
        }

        /// <summary>
        /// Membuat akun baru untuk user Microsoft yang belum terdaftar (Just-In-Time),
        /// dengan role default dari konfigurasi. Mengembalikan null bila role tidak ada.
        /// </summary>
        private async Task<Models.Entities.User?> ProvisionExternalUserAsync(
            string email, string? displayName, CancellationToken cancellationToken)
        {
            var role = await _roles.GetByNameAsync(_authOptions.AutoProvisionRole, cancellationToken);
            if (role is null)
            {
                _logger.LogError("AutoProvision gagal: role '{Role}' tidak ada di database", _authOptions.AutoProvisionRole);
                return null;
            }

            var username = await GenerateUniqueUsernameAsync(email, cancellationToken);

            var user = new Models.Entities.User
            {
                Username = username,
                Email = email,
                FullName = string.IsNullOrWhiteSpace(displayName) ? username : displayName!.Trim(),
                // Login manual tidak dipakai untuk akun SSO; hash acak agar tidak bisa ditebak.
                PasswordHash = _passwordHasher.Hash(Guid.NewGuid().ToString("N")),
                IsActive = true,
                CreatedBy = "MICROSOFT_SSO"
            };

            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            await _users.ReplaceRolesAsync(user.Id, new[] { role.Id }, "MICROSOFT_SSO", cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AutoProvision: akun {Username} ({Email}) dibuat dengan role {Role}",
                username, email, role.Name);

            // Muat ulang lengkap dengan role agar klaim JWT bisa langsung disusun.
            return await _users.GetByEmailWithRolesAsync(email, cancellationToken) ?? user;
        }

        /// <summary>Username dari bagian lokal email; ditambah angka bila sudah dipakai.</summary>
        private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken cancellationToken)
        {
            var atIndex = email.IndexOf('@');
            var baseName = (atIndex > 0 ? email[..atIndex] : email).Trim();
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "user";
            }

            var candidate = baseName;
            var suffix = 1;
            while (await _users.UsernameExistsAsync(candidate, cancellationToken: cancellationToken))
            {
                candidate = $"{baseName}{++suffix}";
            }

            return candidate;
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
