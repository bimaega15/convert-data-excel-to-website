using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sifp_Vue.Server.Helpers
{
    /// <summary>Isi token tantangan MFA setelah divalidasi (lihat <see cref="IMfaChallengeTokenService"/>).</summary>
    public record MfaChallengePayload(int UserId, bool SetupRequired, string? PendingSecret, bool RememberMe);

    public interface IMfaChallengeTokenService
    {
        string Issue(int userId, bool setupRequired, string? pendingSecret, bool rememberMe);
        MfaChallengePayload? Validate(string token);
    }

    /// <summary>
    /// Token berumur pendek (5 menit) yang menjembatani "password benar" dengan
    /// "kode MFA benar", tanpa perlu tabel sesi sementara di database. Ditandatangani
    /// dengan kunci JWT yang sama seperti token bearer biasa, tapi audience berbeda
    /// (lihat <see cref="Audience"/> dan validasi JwtBearer di Program.cs) supaya token
    /// ini tidak bisa dipakai untuk memanggil endpoint API yang butuh login penuh.
    /// </summary>
    public class MfaChallengeTokenService : IMfaChallengeTokenService
    {
        public const string Audience = "SifpAssurance.MfaChallenge";

        private const string PurposeClaim = "purpose";
        private const string PurposeValue = "mfa_challenge";
        private const string SetupClaim = "setup";
        private const string SecretClaim = "pending_secret";
        private const string RememberClaim = "remember";

        private readonly string _issuer;
        private readonly SymmetricSecurityKey _key;
        private readonly SigningCredentials _signingCredentials;

        public MfaChallengeTokenService(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            _issuer = jwtSection["Issuer"] ?? "SifpAssurance";

            var signingKey = jwtSection["SigningKey"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("Jwt:SigningKey belum diatur.");
            }

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            _signingCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        }

        public string Issue(int userId, bool setupRequired, string? pendingSecret, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(PurposeClaim, PurposeValue),
                new(SetupClaim, setupRequired ? "1" : "0"),
                new(RememberClaim, rememberMe ? "1" : "0"),
            };

            // Secret pending ikut ditandatangani di dalam token, bukan disimpan ke
            // database, supaya MfaEnabled baru menyala setelah kode pertama terbukti
            // cocok (lihat AuthService.VerifyMfaAsync).
            if (setupRequired && !string.IsNullOrWhiteSpace(pendingSecret))
            {
                claims.Add(new Claim(SecretClaim, pendingSecret));
            }

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public MfaChallengePayload? Validate(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            // MapInboundClaims=false: tanpa ini, handler diam-diam mengganti nama klaim
            // pendek ("sub") jadi URI panjang (ClaimTypes.NameIdentifier) sebelum
            // FindFirstValue di bawah sempat membacanya, membuat validasi selalu gagal.
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }

            if (principal.FindFirstValue(PurposeClaim) != PurposeValue)
            {
                return null;
            }

            if (!int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            {
                return null;
            }

            var setupRequired = principal.FindFirstValue(SetupClaim) == "1";
            var rememberMe = principal.FindFirstValue(RememberClaim) == "1";
            var pendingSecret = principal.FindFirstValue(SecretClaim);

            return new MfaChallengePayload(userId, setupRequired, pendingSecret, rememberMe);
        }
    }
}
