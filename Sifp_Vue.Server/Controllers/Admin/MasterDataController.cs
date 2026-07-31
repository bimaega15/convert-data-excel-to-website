using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>
    /// Halaman daftar untuk master data yang berasal dari import Excel.
    /// Tabelnya read-only: perubahan dilakukan dengan mengunggah workbook baru,
    /// sehingga hasil di aplikasi selalu bisa ditelusuri ke satu file sumber.
    /// </summary>
    [Route("admin/master")]
    public class MasterDataController : AdminBaseController
    {
        private readonly IMasterDataService _service;

        public MasterDataController(IMasterDataService service)
        {
            _service = service;
        }

        [HttpGet("sif-questions")]
        public async Task<IActionResult> SifQuestions([FromQuery] SifQuestionQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetSifQuestionsAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "SIF Questions",
                Subtitle = "Jawaban pertanyaan verifikasi SIF per observasi",
                RouteName = "sif-questions",
                Columns = new List<string> { "Obs ID", "Protocol", "Ref", "CCVC ID", "Pertanyaan", "Jawaban", "SIF Exposure", "Critical Safeguard", "Komentar" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.ObsId, x.ProtocolCode, x.QuestionRef, x.CcvcId, x.Question, x.Answer, x.SifExposure, x.CriticalSafeguard, x.Comments
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search,
                ObsCode = query.ObsCode
            });
        }

        [HttpGet("error-traps")]
        public async Task<IActionResult> ErrorTraps([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetErrorTrapsAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "Error Traps",
                Subtitle = "Error trap yang teridentifikasi per observasi",
                RouteName = "error-traps",
                Columns = new List<string> { "Obs ID", "Protocol", "Kategori", "Error Trap", "Komentar" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.ObsId, x.ProtocolCode, x.Category, x.ErrorTrap, x.Comments
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search,
                ObsCode = query.ObsCode
            });
        }

        [HttpGet("hp-tools")]
        public async Task<IActionResult> HpTools([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetHpToolsAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "HP Tools",
                Subtitle = "Human Performance tools yang dipakai di lapangan",
                RouteName = "hp-tools",
                Columns = new List<string> { "Obs ID", "Protocol", "Tool", "Tujuan", "Kapan Digunakan", "Cara Pakai", "Catatan Efektivitas" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.ObsId, x.ProtocolCode, x.Tool, x.Tujuan, x.KapanDigunakan, x.CaraPakai, x.EffectivenessNotes
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search,
                ObsCode = query.ObsCode
            });
        }

        [HttpGet("drift-conditions")]
        public async Task<IActionResult> DriftConditions([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetDriftConditionsAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "Drift Conditions",
                Subtitle = "Kondisi drift yang teramati",
                RouteName = "drift-conditions",
                Columns = new List<string> { "Obs ID", "Protocol", "Situasi", "Level 1", "Kode", "Level 2", "Alasan", "Status" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.ObsId, x.ProtocolCode, x.Situation, x.Level1, x.Code, x.Level2, x.Reason, x.Status
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search,
                ObsCode = query.ObsCode
            });
        }

        [HttpGet("latent-conditions")]
        public async Task<IActionResult> LatentConditions([FromQuery] MasterDataQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetLatentConditionsAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "Latent Conditions",
                Subtitle = "Kondisi laten yang teridentifikasi",
                RouteName = "latent-conditions",
                Columns = new List<string> { "Obs ID", "Protocol", "Observasi", "Level 1", "Kode", "Level 2", "Alasan", "Status" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.ObsId, x.ProtocolCode, x.Observation, x.Level1, x.Code, x.Level2, x.Reason, x.Status
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search,
                ObsCode = query.ObsCode
            });
        }

        [HttpGet("ccvc-library")]
        public async Task<IActionResult> CcvcLibrary([FromQuery] CcvcLibraryQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetCcvcLibraryAsync(query, cancellationToken);

            return View("~/Views/Admin/MasterData/Table.cshtml", new MasterDataTableViewModel
            {
                Title = "PSEC & CCVC Library",
                Subtitle = "Master library referensi PSEC dan CCVC",
                RouteName = "ccvc-library",
                Columns = new List<string> { "No", "Protocol Group", "PSEC ID", "PSEC Name", "Exposure Type", "CCVC ID", "Kode", "Ringkasan", "Tujuan Verifikasi" },
                Rows = result.Items.Select(x => new List<string?>
                {
                    x.No?.ToString(), x.ProtocolGroup, x.PsecId, x.PsecName, x.ExposureType, x.CcvcId, x.QuestionCode, x.QuestionSummary, x.VerificationPurpose
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Search = query.Search
            });
        }
    }
}
