using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Controllers.Api
{
    /// <summary>
    /// "Sign in with Microsoft" (Microsoft Entra ID / Azure AD, alur OpenID Connect).
    /// Berbeda dengan <see cref="AuthController"/> yang membalas JSON, endpoint di sini
    /// bekerja lewat redirect browser: login → Microsoft → callback → JWT aplikasi →
    /// kembali ke aplikasi Vue.
    /// </summary>
    [Route("api/auth/microsoft")]
    public class MicrosoftAuthController : Controller
    {
        // camelCase supaya cocok dengan yang diharapkan setSession() di klien Vue.
        private static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

        private readonly IAuthService _authService;
        private readonly AuthOptions _authOptions;
        private readonly AzureAdOptions _azureAd;
        private readonly ILogger<MicrosoftAuthController> _logger;

        public MicrosoftAuthController(
            IAuthService authService,
            IOptions<AuthOptions> authOptions,
            IOptions<AzureAdOptions> azureAd,
            ILogger<MicrosoftAuthController> logger)
        {
            _authService = authService;
            _authOptions = authOptions.Value;
            _azureAd = azureAd.Value;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/auth/microsoft/login — memicu challenge OIDC yang me-redirect
        /// browser ke halaman login Microsoft.
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!_azureAd.IsConfigured)
            {
                return RedirectToClient("/login", ssoError:
                    "Login Microsoft belum dikonfigurasi. Hubungi admin (AzureAd:TenantId/ClientId).");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)) ?? "/api/auth/microsoft/callback"
            };

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                props.Items["returnUrl"] = returnUrl;
            }

            return Challenge(props, AzureAdOptions.OidcScheme);
        }

        /// <summary>
        /// GET /api/auth/microsoft/callback — dijalankan setelah middleware OIDC
        /// menyelesaikan handshake. Menukar email Microsoft dengan JWT aplikasi lalu
        /// menyerahkannya ke klien Vue lewat fragment URL.
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            if (!_azureAd.IsConfigured)
            {
                return RedirectToClient("/login", ssoError: "Login Microsoft belum dikonfigurasi.");
            }

            var auth = await HttpContext.AuthenticateAsync(AzureAdOptions.OidcCookieScheme);
            if (!auth.Succeeded || auth.Principal is null)
            {
                _logger.LogWarning("Callback Microsoft: autentikasi cookie sementara gagal.");
                return RedirectToClient("/login", ssoError: "Login Microsoft gagal atau dibatalkan.");
            }

            // Entra ID v2.0: UPN/email biasanya ada di preferred_username.
            var email = auth.Principal.FindFirstValue("preferred_username")
                        ?? auth.Principal.FindFirstValue(ClaimTypes.Upn)
                        ?? auth.Principal.FindFirstValue("email")
                        ?? auth.Principal.FindFirstValue(ClaimTypes.Email);

            // Nama tampilan untuk mengisi FullName saat akun dibuat otomatis.
            var displayName = auth.Principal.FindFirstValue("name")
                              ?? auth.Principal.FindFirstValue(ClaimTypes.GivenName);

            var result = await _authService.AuthenticateExternalEmailAsync(email ?? string.Empty, displayName);

            // Cookie sementara sudah tidak diperlukan setelah email diambil.
            await HttpContext.SignOutAsync(AzureAdOptions.OidcCookieScheme);

            if (result.Status != ApiStatus.Success || result.Data is null)
            {
                return RedirectToClient("/login", ssoError: result.Message);
            }

            var returnUrl = auth.Properties?.Items.TryGetValue("returnUrl", out var r) == true ? r : null;
            var payload = EncodePayload(result.Data);
            return RedirectToClient("/auth/callback", returnUrl: returnUrl, fragment: $"sso={payload}");
        }

        /// <summary>Serialisasi hasil login menjadi token base64url yang aman ditaruh di fragment URL.</summary>
        private static string EncodePayload(LoginResultDto data)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(data, JsonWeb);
            return WebEncoders.Base64UrlEncode(json);
        }

        /// <summary>
        /// Redirect ke aplikasi Vue. ClientBaseUrl kosong = origin yang sama (produksi);
        /// saat dev diarahkan ke http://localhost:5173.
        /// </summary>
        private IActionResult RedirectToClient(string path, string? ssoError = null, string? returnUrl = null, string? fragment = null)
        {
            var sb = new StringBuilder();
            var baseUrl = _authOptions.ClientBaseUrl?.TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                sb.Append(baseUrl);
            }
            sb.Append(path);

            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(ssoError))
            {
                query.Add("ssoError=" + Uri.EscapeDataString(ssoError));
            }
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                query.Add("returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            if (query.Count > 0)
            {
                sb.Append('?').Append(string.Join('&', query));
            }

            if (!string.IsNullOrWhiteSpace(fragment))
            {
                sb.Append('#').Append(fragment);
            }

            return Redirect(sb.ToString());
        }
    }
}
