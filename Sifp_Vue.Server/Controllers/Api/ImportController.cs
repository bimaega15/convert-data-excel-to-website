using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// Menerima workbook dari halaman Import Excel di Vue.
    /// Kontrak multipart mengikuti <c>submitWorkbook()</c> di
    /// <c>sifp_vue.client/src/services/excelImport.js</c>: field <c>file</c>,
    /// <c>summary</c>, dan <c>edits</c>.
    /// </summary>
    [Route("api/import")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImportController : ApiControllerBase
    {
        private readonly IExcelImportService _importService;

        public ImportController(IExcelImportService importService)
        {
            _importService = importService;
        }

        /// <summary>POST /api/import/excel — memproses workbook dan mengganti seluruh master data.</summary>
        [HttpPost("excel")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportExcel(
            IFormFile? file,
            [FromForm] string? summary,
            [FromForm] string? edits,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return Failure<ImportResultDto>("File tidak ditemukan pada request. Kirim sebagai field \"file\".");
            }

            await using var stream = file.OpenReadStream();

            var result = await _importService.ImportAsync(
                stream, file.FileName, summary, edits, CurrentUserName, cancellationToken);

            return FromResult(result);
        }

        /// <summary>GET /api/import/batches — riwayat import.</summary>
        [HttpGet("batches")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ImportBatchDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBatches([FromQuery] QueryParameters query, CancellationToken cancellationToken)
            => Success(await _importService.GetBatchesAsync(query, cancellationToken));

        /// <summary>GET /api/import/batches/{id} — detail satu proses import.</summary>
        [HttpGet("batches/{id:int}")]
        public async Task<IActionResult> GetBatch(int id, CancellationToken cancellationToken)
        {
            var batch = await _importService.GetBatchAsync(id, cancellationToken);
            return batch is null
                ? Failure<ImportBatchDto>("Batch import tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(batch);
        }

        /// <summary>
        /// GET /api/import/required-sheets — daftar sheet wajib beserta labelnya.
        /// Klien bisa memakainya untuk validasi awal tanpa menduplikasi daftar di dua tempat.
        /// </summary>
        [HttpGet("required-sheets")]
        public IActionResult RequiredSheets()
            => Success(Helpers.SheetSchema.RequiredSheets.Select(s => new { name = s.Name, label = s.Label }));
    }
}
