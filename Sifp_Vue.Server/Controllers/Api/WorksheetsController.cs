using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// Menggantikan file <c>src/data/generated/sheets/*.json</c> di klien:
    /// manifest untuk menu sidebar, dan data mentah untuk viewer worksheet generik.
    /// </summary>
    [Route("api/worksheets")]
    public class WorksheetsController : ApiControllerBase
    {
        private readonly IWorksheetService _service;

        public WorksheetsController(IWorksheetService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /api/worksheets/manifest — daftar worksheet dari import terakhir,
        /// sudah dikelompokkan sesuai urutan grup sidebar.
        /// </summary>
        [HttpGet("manifest")]
        [ProducesResponseType(typeof(ApiResponse<WorksheetManifestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Manifest(CancellationToken cancellationToken)
            => Success(await _service.GetManifestAsync(cancellationToken));

        /// <summary>GET /api/worksheets/{slug} — isi mentah satu worksheet.</summary>
        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(ApiResponse<WorksheetDataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<WorksheetDataDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            var sheet = await _service.GetBySlugAsync(slug, cancellationToken);
            return sheet is null
                ? Failure<WorksheetDataDto>($"Worksheet \"{slug}\" tidak ditemukan.", StatusCodes.Status404NotFound)
                : Success(sheet);
        }
    }
}
