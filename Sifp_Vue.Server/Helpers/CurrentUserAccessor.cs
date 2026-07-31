using System.Security.Claims;
using Sifp_Vue.Server.Data;

namespace Sifp_Vue.Server.Helpers
{
    /// <summary>
    /// Implementasi <see cref="ICurrentUserAccessor"/> berbasis HttpContext.
    /// Dipisah dari DbContext supaya migration dan seeder (yang berjalan tanpa
    /// request HTTP) tetap bisa memakai DbContext yang sama.
    /// </summary>
    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "SYSTEM";

        public int? UserId
        {
            get
            {
                var raw = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(raw, out var id) ? id : null;
            }
        }
    }
}
