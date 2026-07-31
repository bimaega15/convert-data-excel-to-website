using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Models.ViewModels;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>Manajemen user. Hanya role Administrator, lebih ketat dari halaman admin lain.</summary>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = RoleNames.Administrator)]
    [Route("admin/users")]
    public class UsersController : AdminBaseController
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] QueryParameters query, CancellationToken cancellationToken)
        {
            var model = new UserListViewModel
            {
                Title = "Users",
                Subtitle = "Akun dan hak akses aplikasi",
                Query = query,
                Result = await _service.GetPagedAsync(query, cancellationToken)
            };

            return View("~/Views/Admin/Users/Index.cshtml", model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var model = new UserFormViewModel
            {
                AvailableRoles = await _service.GetRolesAsync(cancellationToken)
            };

            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _service.GetRolesAsync(cancellationToken);
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            var result = await _service.CreateAsync(model.CreateForm, CurrentUserName, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.AvailableRoles = await _service.GetRolesAsync(cancellationToken);
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var user = await _service.GetByIdAsync(id, cancellationToken);
            if (user is null)
            {
                SetError("User tidak ditemukan.");
                return RedirectToAction(nameof(Index));
            }

            var roles = await _service.GetRolesAsync(cancellationToken);

            var model = new UserFormViewModel
            {
                Id = id,
                Username = user.Username,
                AvailableRoles = roles,
                EditForm = new UpdateUserRequest
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Zona = user.Zona,
                    IsActive = user.IsActive,
                    RoleIds = roles.Where(r => user.Roles.Contains(r.Name)).Select(r => r.Id).ToList()
                }
            };

            return View("~/Views/Admin/Users/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model, CancellationToken cancellationToken)
        {
            model.Id = id;

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _service.GetRolesAsync(cancellationToken);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }

            var result = await _service.UpdateAsync(id, model.EditForm, CurrentUserName, cancellationToken);
            if (result.Status != ApiStatus.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.AvailableRoles = await _service.GetRolesAsync(cancellationToken);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }

            SetSuccess(result.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == id)
            {
                return Json(new { success = false, message = "Anda tidak dapat menghapus akun yang sedang dipakai." });
            }

            var result = await _service.DeleteAsync(id, cancellationToken);
            return Json(new { success = result.Status == ApiStatus.Success, message = result.Message });
        }
    }
}
