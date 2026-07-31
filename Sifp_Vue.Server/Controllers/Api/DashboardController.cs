using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    [Route("api/dashboard")]
    public class DashboardController : ApiControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// GET /api/dashboard — payload dashboard lengkap.
        /// Bentuknya sama persis dengan <c>src/data/generated/dashboard.json</c>,
        /// jadi <c>src/data/dashboard.js</c> cukup mengganti sumber datanya ke sini.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var dashboard = await _dashboardService.GetDashboardAsync(cancellationToken);
            return Success(dashboard);
        }

        /// <summary>GET /api/dashboard/summary — ringkasan angka untuk kartu admin.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        {
            var summary = await _dashboardService.GetAdminSummaryAsync(cancellationToken);
            return Success(summary);
        }
    }
}
