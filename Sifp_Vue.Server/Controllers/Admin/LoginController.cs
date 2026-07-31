using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Admin
{
    /// <summary>Login/logout cookie untuk area admin. Sengaja tidak mewarisi AdminBaseController.</summary>
    [AllowAnonymous]
    [Route("admin")]
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Index(string? returnUrl = null)
        {
            // Sesi lama dibersihkan supaya membuka /admin/login selalu memberi form kosong.
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Admin/Login/Index.cshtml", new LoginRequest());
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginRequest model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Login/Index.cshtml", model);
            }

            var result = await _authService.AuthenticateForAdminAsync(
                model, CookieAuthenticationDefaults.AuthenticationScheme, HttpContext.RequestAborted);

            if (result.Status != ApiStatus.Success || result.Data is null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Admin/Login/Index.cshtml", model);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(result.Data),
                new AuthenticationProperties { IsPersistent = false });

            // Hanya URL lokal yang diterima, agar parameter returnUrl tidak bisa
            // dipakai mengarahkan user ke situs luar setelah login.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet("denied")]
        public IActionResult Denied() => View("~/Views/Admin/Login/Denied.cshtml");
    }
}
