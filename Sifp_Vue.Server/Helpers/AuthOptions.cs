namespace Sifp_Vue.Server.Helpers
{
    /// <summary>
    /// Aturan autentikasi tingkat aplikasi. Diisi dari section "Auth" di appsettings.
    /// </summary>
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        /// <summary>
        /// Origin aplikasi Vue tujuan redirect setelah login Microsoft selesai.
        /// Kosong = origin yang sama dengan server (mode produksi, Vue disajikan
        /// dari wwwroot). Saat dev diisi <c>http://localhost:5173</c>.
        /// </summary>
        public string ClientBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Bila true, akun yang login lewat Microsoft dengan domain yang diizinkan
        /// namun belum ada di database akan dibuat otomatis (Just-In-Time), sehingga
        /// setiap karyawan @pertamina.com bisa langsung masuk tanpa didaftarkan admin.
        /// Bila false, hanya akun yang sudah terdaftar yang diterima.
        /// </summary>
        public bool AutoProvision { get; set; }

        /// <summary>Role default untuk akun yang dibuat otomatis (lihat <see cref="AutoProvision"/>).</summary>
        public string AutoProvisionRole { get; set; } = "Viewer";

        /// <summary>Domain yang dipakai bila konfigurasi tidak mengisi apa pun.</summary>
        private static readonly string[] FallbackDomains = { "pertamina.com" };

        /// <summary>
        /// Daftar domain email yang boleh login. Semua akun (login manual maupun
        /// Windows SSO) wajib memiliki email di salah satu domain ini. Isi dengan
        /// <c>"*"</c> untuk mengizinkan semua domain (mematikan pembatasan).
        /// Default (kosong) => hanya <c>pertamina.com</c>.
        /// </summary>
        /// <remarks>
        /// Sengaja dibiarkan kosong sebagai default: binder konfigurasi .NET
        /// menambahkan (append) item array dari appsettings ke nilai default,
        /// sehingga default non-kosong akan menggandakan entri.
        /// </remarks>
        public string[] AllowedEmailDomains { get; set; } = System.Array.Empty<string>();

        /// <summary>Domain efektif setelah menerapkan fallback aman.</summary>
        public string[] EffectiveDomains =>
            AllowedEmailDomains is { Length: > 0 } ? AllowedEmailDomains : FallbackDomains;

        /// <summary>True bila email berada di salah satu domain yang diizinkan.</summary>
        public bool IsEmailDomainAllowed(string? email)
        {
            var domains = EffectiveDomains;

            // "*" mematikan pembatasan domain.
            if (domains.Any(d => d.Trim() == "*"))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var atIndex = email.LastIndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                return false;
            }

            var domain = email[(atIndex + 1)..].Trim();
            return domains.Any(d =>
                !string.IsNullOrWhiteSpace(d) &&
                string.Equals(domain, d.Trim(), System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
