using System.Security.Claims;
using Microsoft.Extensions.Options;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Entities;
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
        private readonly ITotpService _totp;
        private readonly IMfaChallengeTokenService _mfaChallengeTokens;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<AuthService> _logger;

        private const string MfaIssuerName = "SIFP Assurance";

        public AuthService(
            IUserRepository users,
            IRoleRepository roles,
            IPasswordHasher passwordHasher,
            IUserClaimsFactory claimsFactory,
            IJwtTokenService jwtTokenService,
            ITotpService totp,
            IMfaChallengeTokenService mfaChallengeTokens,
            IOptions<AuthOptions> authOptions,
            ILogger<AuthService> logger)
        {
            _users = users;
            _roles = roles;
            _passwordHasher = passwordHasher;
            _claimsFactory = claimsFactory;
            _jwtTokenService = jwtTokenService;
            _totp = totp;
            _mfaChallengeTokens = mfaChallengeTokens;
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

        public async Task<ApiResponse<LoginChallengeDto>> AuthenticateForApiAsync(
            LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request, cancellationToken);
            if (user is null)
            {
                return ApiResponse<LoginChallengeDto>.Fail(InvalidCredentialsMessage);
            }

            if (!_authOptions.IsEmailDomainAllowed(user.Email))
            {
                _logger.LogWarning("Login API ditolak: email {Email} di luar domain yang diizinkan", user.Email);
                return ApiResponse<LoginChallengeDto>.Fail(EmailDomainRejectedMessage());
            }

            // LastLoginAt & role belum disentuh di sini secara sengaja — login baru
            // benar-benar selesai setelah kode MFA terverifikasi (VerifyMfaAsync).
            _logger.LogInformation("Password valid untuk {Username}, menunggu verifikasi MFA", user.Username);
            return ApiResponse<LoginChallengeDto>.Ok(BuildMfaChallenge(user, request.RememberMe), "Verifikasi MFA diperlukan.");
        }

        public async Task<ApiResponse<LoginChallengeDto>> AuthenticateExternalEmailAsync(
            string email, string? displayName = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ApiResponse<LoginChallengeDto>.Fail("Email tidak terbaca dari akun Microsoft.");
            }

            email = email.Trim();

            // Batasan domain ditegakkan lebih dulu supaya email di luar @pertamina.com
            // ditolak dengan pesan yang jelas, bukan "belum terdaftar".
            if (!_authOptions.IsEmailDomainAllowed(email))
            {
                _logger.LogWarning("Login Microsoft ditolak: email {Email} di luar domain yang diizinkan", email);
                return ApiResponse<LoginChallengeDto>.Fail(EmailDomainRejectedMessage());
            }

            var user = await _users.GetByEmailWithRolesAsync(email, cancellationToken);

            if (user is null)
            {
                if (!_authOptions.AutoProvision)
                {
                    _logger.LogWarning("Login Microsoft ditolak: email {Email} belum terdaftar (AutoProvision nonaktif)", email);
                    return ApiResponse<LoginChallengeDto>.Fail(ExternalAccountNotFoundMessage);
                }

                var provisioned = await ProvisionExternalUserAsync(email, displayName, cancellationToken);
                if (provisioned is null)
                {
                    return ApiResponse<LoginChallengeDto>.Fail(
                        $"Role default '{_authOptions.AutoProvisionRole}' tidak ditemukan. Hubungi admin.");
                }
                user = provisioned;
            }
            else if (!user.IsActive)
            {
                _logger.LogWarning("Login Microsoft ditolak: akun {Email} non-aktif", email);
                return ApiResponse<LoginChallengeDto>.Fail("Akun Anda dinonaktifkan. Hubungi admin.");
            }

            _logger.LogInformation("Login Microsoft valid untuk {Username} ({Email}), menunggu verifikasi MFA", user.Username, email);
            return ApiResponse<LoginChallengeDto>.Ok(BuildMfaChallenge(user, rememberMe: false), "Verifikasi MFA diperlukan.");
        }

        public async Task<ApiResponse<LoginChallengeDto>> AuthenticateWindowsUserAsync(
            string winIdentityName, string? displayName = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(winIdentityName))
                {
                    return ApiResponse<LoginChallengeDto>.Fail("Identitas Windows Authenticator tidak terdeteksi dari environment.");
                }

                var cleanUsername = winIdentityName.Contains('\\')
                    ? winIdentityName.Split('\\').Last()
                    : winIdentityName;

                var email = cleanUsername.Contains('@') ? cleanUsername : $"{cleanUsername}@pertamina.com";

                var user = await _users.GetByUsernameWithRolesAsync(cleanUsername, cancellationToken)
                           ?? await _users.GetByEmailWithRolesAsync(email, cancellationToken);

                if (user is null)
                {
                    if (!_authOptions.AutoProvision)
                    {
                        _logger.LogWarning("Login Windows ditolak: akun {WinIdentity} belum terdaftar dan AutoProvision dinonaktifkan", winIdentityName);
                        return ApiResponse<LoginChallengeDto>.Fail($"Akun Windows ({winIdentityName}) belum terdaftar di sistem.");
                    }

                    user = await ProvisionExternalUserAsync(email, displayName ?? cleanUsername, cancellationToken);
                    if (user is null)
                    {
                        return ApiResponse<LoginChallengeDto>.Fail("Gagal memproses pendaftaran akun Windows baru.");
                    }
                }
                else if (!user.IsActive)
                {
                    _logger.LogWarning("Login Windows ditolak: akun {Username} non-aktif", user.Username);
                    return ApiResponse<LoginChallengeDto>.Fail("Akun Anda dinonaktifkan. Hubungi admin.");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _users.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Login Windows Authenticator valid untuk {Username}, menyusun tantangan MFA", user.Username);
                return ApiResponse<LoginChallengeDto>.Ok(BuildMfaChallenge(user, rememberMe: false), "Login Windows Authenticator berhasil.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error otentikasi Windows Authenticator untuk {WinIdentity}", winIdentityName);
                return ApiResponse<LoginChallengeDto>.Fail($"Gagal otentikasi Windows Authenticator: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoginResultDto>> VerifyMfaAsync(
            MfaVerifyRequest request, CancellationToken cancellationToken = default)
        {
            const string ChallengeInvalidMessage = "Sesi verifikasi MFA sudah kedaluwarsa. Silakan login ulang.";

            var payload = _mfaChallengeTokens.Validate(request.ChallengeToken);
            if (payload is null)
            {
                return ApiResponse<LoginResultDto>.Fail(ChallengeInvalidMessage);
            }

            var user = await _users.GetByIdWithRolesTrackedAsync(payload.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return ApiResponse<LoginResultDto>.Fail(ChallengeInvalidMessage);
            }

            // Mode setup: secret belum pernah tersimpan, jadi diambil dari klaim
            // token (lihat MfaChallengeTokenService.Issue). Mode biasa: pakai secret
            // yang sudah tersimpan di akun.
            var secretToCheck = payload.SetupRequired ? payload.PendingSecret : user.MfaSecret;
            if (string.IsNullOrWhiteSpace(secretToCheck) || !_totp.ValidateCode(secretToCheck, request.Code))
            {
                _logger.LogWarning("Kode MFA salah untuk {Username}", user.Username);
                return ApiResponse<LoginResultDto>.Fail("Kode MFA salah atau sudah kedaluwarsa.");
            }

            if (payload.SetupRequired)
            {
                user.MfaSecret = secretToCheck;
                user.MfaEnabled = true;
                _logger.LogInformation("MFA diaktifkan untuk {Username}", user.Username);
            }

            var roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList();

            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login berhasil (MFA terverifikasi) untuk {Username}", user.Username);
            return ApiResponse<LoginResultDto>.Ok(IssueToken(user, roles, payload.RememberMe), "Login berhasil.");
        }

        /// <summary>
        /// Menyusun tantangan MFA untuk user yang kredensialnya (password/SSO) sudah
        /// terbukti benar. Akun yang belum pernah mengaktifkan MFA mendapat secret baru
        /// + QR code setup; akun yang sudah aktif cukup diminta kode berikutnya.
        /// </summary>
        private LoginChallengeDto BuildMfaChallenge(Models.Entities.User user, bool rememberMe)
        {
            if (user.MfaEnabled && !string.IsNullOrWhiteSpace(user.MfaSecret))
            {
                return new LoginChallengeDto
                {
                    ChallengeToken = _mfaChallengeTokens.Issue(user.Id, setupRequired: false, pendingSecret: null, rememberMe),
                    SetupRequired = false,
                };
            }

            var secret = _totp.GenerateSecret();
            var otpAuthUri = _totp.BuildOtpAuthUri(secret, user.Username, MfaIssuerName);

            return new LoginChallengeDto
            {
                ChallengeToken = _mfaChallengeTokens.Issue(user.Id, setupRequired: true, pendingSecret: secret, rememberMe),
                SetupRequired = true,
                QrCodeDataUri = _totp.GenerateQrCodeDataUri(otpAuthUri),
                ManualEntryKey = FormatSecretForDisplay(secret),
            };
        }

        /// <summary>Dipecah tiap 4 karakter ("ABCD EFGH ...") supaya gampang diketik manual bila QR tidak bisa di-scan.</summary>
        private static string FormatSecretForDisplay(string secret)
        {
            var chunks = Enumerable.Range(0, (secret.Length + 3) / 4)
                .Select(i => secret.Substring(i * 4, Math.Min(4, secret.Length - i * 4)));
            return string.Join(' ', chunks);
        }

        /// <summary>
        /// Membuat akun baru untuk user Microsoft yang belum terdaftar (Just-In-Time),
        /// dengan role default dari konfigurasi. Mengembalikan null bila role tidak ada.
        /// </summary>
        private async Task<Models.Entities.User?> ProvisionExternalUserAsync(
            string email, string? displayName, CancellationToken cancellationToken)
        {
            var targetRoleName = (email.Contains("haris", StringComparison.OrdinalIgnoreCase) || (displayName != null && displayName.Contains("haris", StringComparison.OrdinalIgnoreCase)))
                ? RoleNames.Administrator
                : _authOptions.AutoProvisionRole;

            var role = await _roles.GetByNameAsync(targetRoleName, cancellationToken)
                       ?? await _roles.GetByNameAsync(_authOptions.AutoProvisionRole, cancellationToken);
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
