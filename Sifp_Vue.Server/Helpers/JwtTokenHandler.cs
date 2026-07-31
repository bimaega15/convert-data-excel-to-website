using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Helpers
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "Sifp_Vue.Server";
        public string Audience { get; set; } = "sifp_vue.client";
        public int ExpiryMinutes { get; set; } = 480;
    }

    public interface IJwtTokenHandler
    {
        /// <summary>Membuat access token beserta waktu kedaluwarsanya (UTC).</summary>
        (string Token, DateTime ExpiresAtUtc) CreateToken(User user, IEnumerable<string> roles);

        /// <summary>Daftar claim yang sama, dipakai ulang untuk cookie sign-in area admin.</summary>
        ClaimsIdentity BuildIdentity(User user, IEnumerable<string> roles, string authenticationScheme);
    }

    public class JwtTokenHandler : IJwtTokenHandler
    {
        public const string ZonaClaimType = "zona";

        private readonly JwtOptions _options;

        public JwtTokenHandler(Microsoft.Extensions.Options.IOptions<JwtOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.Key) || Encoding.UTF8.GetByteCount(_options.Key) < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Key wajib diisi dan minimal 32 byte. Atur lewat user-secrets atau environment variable.");
            }
        }

        public (string Token, DateTime ExpiresAtUtc) CreateToken(User user, IEnumerable<string> roles)
        {
            var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: BuildClaims(user, roles),
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public ClaimsIdentity BuildIdentity(User user, IEnumerable<string> roles, string authenticationScheme)
        {
            return new ClaimsIdentity(BuildClaims(user, roles), authenticationScheme);
        }

        private static List<Claim> BuildClaims(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username)
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim("full_name", user.FullName));
            }

            if (user.Zona.HasValue)
            {
                claims.Add(new Claim(ZonaClaimType, user.Zona.Value.ToString()));
            }

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return claims;
        }
    }
}
