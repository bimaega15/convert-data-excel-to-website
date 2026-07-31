using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Controllers.Admin
{
    [Route("admin/initiatives")]
    public class InitiativesController : AdminBaseController
    {
        private static readonly List<string> KnownStatuses = new()
        {
            "Not Started", "In Progress", "On Hold", "Completed"
        };

        private readonly IInitiativeService _service;

        public InitiativesController(IInitiativeService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] InitiativeQuery query, CancellationToken cancellationToken)
        {
            var model = new InitiativeListViewModel
            {
                Title = "Improvement Initiatives",
                Subtitle = "Inisiatif perbaikan dan progresnya",
                Query = query,
                Result = await _service.GetPagedAsync(query, cancellationToken),
                Statuses = KnownStatuses
            };

            return View("~/Views/Admin/Initiatives/Index.cshtml", model);
        }

        [HttpGet("create")]
        public IActionResult Create()
            => View("~/Views/Admin/Initiatives/Create.cshtml", new InitiativeFormViewModel());

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InitiativeFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Initiatives/Create.cshtml", model);
            }

            var result = await _service.CreateAsync(model.Form, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Admin/Initiatives/Create.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null)
            {
                SetError("Inisiatif tidak ditemukan.");
                return RedirectToAction(nameof(Index));
            }

            var model = new InitiativeFormViewModel
            {
                Id = id,
                Form = new InitiativeRequest
                {
                    ImprovementCode = dto.Id,
                    Initiative = dto.Initiative,
                    RelatedClsr = dto.RelatedClsr,
                    Owner = dto.Owner,
                    Status = dto.Status,
                    ProgressPercent = dto.Progress,
                    ExpectedImpact = dto.ExpectedImpact,
                    Notes = dto.Notes
                }
            };

            return View("~/Views/Admin/Initiatives/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitiativeFormViewModel model, CancellationToken cancellationToken)
        {
            model.Id = id;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Initiatives/Edit.cshtml", model);
            }

            var result = await _service.UpdateAsync(id, model.Form, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Admin/Initiatives/Edit.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(id, cancellationToken);
            return Json(new { success = result.Status == ApiStatus.Success, message = result.Message });
        }
    }
}
