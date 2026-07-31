using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>CRUD observasi versi Razor. Pola aksinya mengikuti Index/Create/Edit/Details/Delete.</summary>
    [Route("admin/observations")]
    public class ObservationsController : AdminBaseController
    {
        private readonly IObservationService _service;

        public ObservationsController(IObservationService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] ObservationQuery query, CancellationToken cancellationToken)
        {
            var options = await _service.GetFilterOptionsAsync(cancellationToken);

            var model = new ObservationListViewModel
            {
                Title = "Observations",
                Subtitle = "Master data hasil observasi V&V",
                Query = query,
                Result = await _service.GetPagedAsync(query, cancellationToken),
                Options = new ObservationFilterOptionsDtoWrapper
                {
                    Zonas = options.Zonas,
                    ProtocolCodes = options.ProtocolCodes,
                    Sites = options.Sites,
                    Companies = options.Companies,
                    Statuses = options.Statuses
                }
            };

            return View("~/Views/Admin/Observations/Index.cshtml", model);
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var detail = await _service.GetDetailAsync(id, cancellationToken);
            if (detail is null)
            {
                SetError("Observasi tidak ditemukan.");
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Observations/Details.cshtml", detail);
        }

        [HttpGet("create")]
        public IActionResult Create()
            => View("~/Views/Admin/Observations/Create.cshtml", new ObservationFormViewModel());

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ObservationFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Observations/Create.cshtml", model);
            }

            var result = await _service.CreateAsync(model.Form, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Admin/Observations/Create.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var detail = await _service.GetDetailAsync(id, cancellationToken);
            if (detail is null)
            {
                SetError("Observasi tidak ditemukan.");
                return RedirectToAction(nameof(Index));
            }

            var model = new ObservationFormViewModel
            {
                Id = id,
                Form = ToRequest(detail.Observation)
            };

            return View("~/Views/Admin/Observations/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ObservationFormViewModel model, CancellationToken cancellationToken)
        {
            model.Id = id;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Observations/Edit.cshtml", model);
            }

            var result = await _service.UpdateAsync(id, model.Form, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Admin/Observations/Edit.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Dipanggil lewat fetch dari modal konfirmasi, jadi merespons JSON.</summary>
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(id, cancellationToken);
            return Json(new { success = result.Status == ApiStatus.Success, message = result.Message });
        }

        /// <summary>
        /// DTO daftar dipakai ulang untuk mengisi form edit. Pemetaan balik ditulis
        /// di sini karena hanya halaman admin yang membutuhkannya.
        /// </summary>
        private static ObservationRequest ToRequest(ObservationDto dto) => new()
        {
            ObsCode = dto.Id,
            ProtocolCode = dto.ProtocolCode,
            ProtocolName = dto.ProtocolName,
            ObservationDate = DateOnly.TryParse(dto.Date, out var date) ? date : null,
            Zona = dto.Zona,
            Site = dto.Site,
            AreaEquipment = dto.Area,
            Activity = dto.Activity,
            Company = dto.Company,
            Observer1 = dto.Observers.ElementAtOrDefault(0),
            Observer2 = dto.Observers.ElementAtOrDefault(1),
            Observer3 = dto.Observers.ElementAtOrDefault(2),
            YesCount = dto.Yes,
            NoCount = dto.No,
            NaCount = dto.Na,
            PerformancePercent = dto.Performance,
            ObservationSequence = dto.Sequence,
            PsieEligible = dto.PsieEligible == "Y",
            Status = dto.Status,
            IsActive = dto.Active == "Y"
        };
    }
}
