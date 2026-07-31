using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>
    /// Basis seluruh halaman /admin. Memakai autentikasi cookie (bukan JWT) karena
    /// area ini dirender server-side dan diakses langsung lewat browser.
    /// </summary>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Policy = "AdminOnly")]
    public abstract class AdminBaseController : Controller
    {
        protected string CurrentUserName => User.Identity?.Name ?? "SYSTEM";

        protected int? CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        protected void SetSuccess(string message) => TempData["SuccessMessage"] = message;

        protected void SetError(string message) => TempData["ErrorMessage"] = message;
    }
}
