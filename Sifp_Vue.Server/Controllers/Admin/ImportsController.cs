using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>Riwayat import workbook beserta form unggah manual dari sisi admin.</summary>
    [Route("admin/imports")]
    public class ImportsController : AdminBaseController
    {
        private readonly IExcelImportService _importService;

        public ImportsController(IExcelImportService importService)
        {
            _importService = importService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] QueryParameters query, CancellationToken cancellationToken)
        {
            var model = new ImportBatchListViewModel
            {
                Title = "Import Excel",
                Subtitle = "Riwayat unggah workbook V&V",
                Query = query,
                Result = await _importService.GetBatchesAsync(query, cancellationToken)
            };

            return View("~/Views/Admin/Imports/Index.cshtml", model);
        }

        [HttpPost("upload")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                SetError("Pilih file workbook terlebih dahulu.");
                return RedirectToAction(nameof(Index));
            }

            await using var stream = file.OpenReadStream();

            // Unggah dari halaman admin tidak melalui layar preview, jadi tidak ada
            // daftar edit sel yang perlu diterapkan.
            var result = await _importService.ImportAsync(
                stream, file.FileName, summaryJson: null, editsJson: null, CurrentUserName, cancellationToken);

            if (result.Status == ApiStatus.Success)
            {
                var warnings = result.Data?.Warnings.Count ?? 0;
                SetSuccess(warnings > 0
                    ? $"{result.Message} ({warnings} peringatan — lihat detail batch)"
                    : result.Message);
            }
            else
            {
                SetError(result.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var batch = await _importService.GetBatchAsync(id, cancellationToken);
            if (batch is null)
            {
                SetError("Batch import tidak ditemukan.");
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Imports/Details.cshtml", batch);
        }
    }
}
