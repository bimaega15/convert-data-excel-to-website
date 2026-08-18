namespace Sifp_Vue.Server.Helpers
{
    /// <summary>
    /// Konfigurasi login "Sign in with Microsoft" (Microsoft Entra ID / Azure AD,
    /// alur OpenID Connect). Diisi dari section "AzureAd" di appsettings.
    /// Nilai sensitif (ClientSecret) sebaiknya lewat user-secrets / environment
    /// variable, bukan disimpan di appsettings.json.
    /// </summary>
    public class AzureAdOptions
    {
        public const string SectionName = "AzureAd";

        /// <summary>Nama skema challenge OpenID Connect ke Microsoft.</summary>
        public const string OidcScheme = "Microsoft";

        /// <summary>
        /// Cookie sementara tempat hasil OIDC ditampung selama handshake, sebelum
        /// ditukar menjadi JWT aplikasi. Terpisah dari cookie admin (/admin).
        /// </summary>
        public const string OidcCookieScheme = "MicrosoftOidcCookie";

        /// <summary>Authority dasar, umumnya <c>https://login.microsoftonline.com/</c>.</summary>
        public string Instance { get; set; } = "https://login.microsoftonline.com/";

        /// <summary>Directory (tenant) ID dari App registration Azure. Single-tenant Pertamina.</summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>Application (client) ID dari App registration.</summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>Client secret dari App registration (Certificates &amp; secrets).</summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Redirect URI path yang didaftarkan di Azure. Harus sama persis, mis.
        /// <c>/signin-oidc</c> → https://host/signin-oidc.
        /// </summary>
        public string CallbackPath { get; set; } = "/signin-oidc";

        /// <summary>Authority lengkap untuk endpoint v2.0 tenant ini.</summary>
        public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}/v2.0";

        /// <summary>True bila tenant + client id sudah diisi sehingga OIDC bisa diaktifkan.</summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(TenantId) && !string.IsNullOrWhiteSpace(ClientId);
    }
}
