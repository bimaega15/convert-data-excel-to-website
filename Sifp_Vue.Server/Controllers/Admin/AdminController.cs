using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>Halaman utama area admin: ringkasan data dan riwayat import terakhir.</summary>
    [Route("admin")]
    public class AdminController : AdminBaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IMasterDataService _masterDataService;
        private readonly IExcelImportService _importService;

        public AdminController(
            IDashboardService dashboardService,
            IMasterDataService masterDataService,
            IExcelImportService importService)
        {
            _dashboardService = dashboardService;
            _masterDataService = masterDataService;
            _importService = importService;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new AdminDashboardViewModel
            {
                Summary = await _dashboardService.GetAdminSummaryAsync(cancellationToken),
                RowCounts = await _masterDataService.GetRowCountsAsync(cancellationToken),
                RecentImports = (await _importService.GetBatchesAsync(
                    new Models.Dtos.QueryParameters { Page = 1, PageSize = 5 }, cancellationToken)).Items
            };

            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }
    }
}
