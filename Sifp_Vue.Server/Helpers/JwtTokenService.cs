using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Helpers
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Token bearer untuk klien Vue, dibangun dari klaim yang sama dengan login cookie admin.
        /// <paramref name="rememberMe"/> memakai masa berlaku yang jauh lebih panjang (Jwt:RememberMeExpiryMinutes).
        /// </summary>
        (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, IEnumerable<string> roles, bool rememberMe = false);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IUserClaimsFactory _claimsFactory;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly int _rememberMeExpiryMinutes;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(IUserClaimsFactory claimsFactory, IConfiguration configuration)
        {
            _claimsFactory = claimsFactory;

            var jwtSection = configuration.GetSection("Jwt");
            _issuer = jwtSection["Issuer"] ?? "SifpAssurance";
            _audience = jwtSection["Audience"] ?? "SifpAssurance.Client";
            _expiryMinutes = jwtSection.GetValue<int?>("ExpiryMinutes") ?? 480;
            // 14 hari secara default -- jauh lebih panjang dari sesi normal (8 jam).
            _rememberMeExpiryMinutes = jwtSection.GetValue<int?>("RememberMeExpiryMinutes") ?? 20_160;

            var signingKey = jwtSection["SigningKey"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException(
                    "Jwt:SigningKey belum diatur. Isi di appsettings.Development.json, user-secrets, " +
                    "atau environment variable Jwt__SigningKey.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, IEnumerable<string> roles, bool rememberMe = false)
        {
            var identity = _claimsFactory.BuildIdentity(user, roles, JwtBearerDefaults.AuthenticationScheme);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(rememberMe ? _rememberMeExpiryMinutes : _expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: identity.Claims,
                expires: expiresAtUtc,
                signingCredentials: _signingCredentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }
}
