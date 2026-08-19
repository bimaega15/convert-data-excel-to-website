using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;

namespace Sifp_Vue.Server.Controllers.Api
{
    [Route("api/roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolesController : ApiControllerBase
    {
        private readonly IRoleRepository _roles;

        public RolesController(IRoleRepository roles)
        {
            _roles = roles;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
            => Success(await _roles.GetAllWithCountsAsync(cancellationToken));

        [HttpGet("permissions")]
        public IActionResult GetRolePermissions()
        {
            var menuAccess = new[]
            {
                new { menu = "Dashboard", roles = new[] { "Administrator", "Verifier", "Viewer" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "Observations", roles = new[] { "Administrator", "Verifier" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "Master Data", roles = new[] { "Administrator", "Verifier" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "Initiatives", roles = new[] { "Administrator", "Verifier" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "Import Excel", roles = new[] { "Administrator" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "User Management", roles = new[] { "Administrator" }, canCreate = true, canEdit = true, canDelete = true },
                new { menu = "System Audit Logs", roles = new[] { "Administrator" }, canCreate = false, canEdit = false, canDelete = false }
            };

            return Success(menuAccess);
        }
    }
}
