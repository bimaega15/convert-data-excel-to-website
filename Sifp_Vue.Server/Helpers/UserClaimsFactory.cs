using System.Security.Claims;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Helpers
{
    public interface IUserClaimsFactory
    {
        /// <summary>Identity siap sign-in untuk cookie login area admin.</summary>
        ClaimsIdentity BuildIdentity(User user, IEnumerable<string> roles, string authenticationScheme);
    }

    public class UserClaimsFactory : IUserClaimsFactory
    {
        public const string ZonaClaimType = "zona";

        public ClaimsIdentity BuildIdentity(User user, IEnumerable<string> roles, string authenticationScheme)
        {
            var claims = new List<Claim>
            {
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

            return new ClaimsIdentity(claims, authenticationScheme);
        }
    }
}
