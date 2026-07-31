using System.Text;
using System.Text.RegularExpressions;

namespace Sifp_Vue.Server.Helpers
{
    public record SheetDefinition(string Name, string Label);

    public record CuratedSheet(string Route, string Label, string Icon);

    /// <summary>
    /// Padanan sisi server dari <c>sifp_vue.client/src/data/sheet-schema.js</c>.
    /// Kedua daftar harus tetap identik: klien memakainya untuk validasi sebelum
    /// upload, server memakainya sebagai gerbang terakhir sebelum data disimpan.
    /// </summary>
    public static class SheetSchema
    {
        public static readonly IReadOnlyList<SheetDefinition> RequiredSheets = new List<SheetDefinition>
        {
            new("INPUT-SIF_Questions", "Jawaban pertanyaan verifikasi SIF"),
            new("INPUT-Error_Traps", "Error traps per observasi"),
            new("INPUT-HP_Tools", "Human Performance Tools"),
            new("INPUT-Drift_Conditions", "Kondisi drift"),
            new("INPUT-Latent_Conditions", "Kondisi laten"),
            new("DATABASE_PSEC_CCVC", "Master library PSEC & CCVC"),
            new("ANALYZE-CONFORMANCE_SCORE", "Rekap observasi & skor"),
            new("ANALYZE-EXECUTIVE_MEASURES", "KPI PSEC / CCVC / PSIE / Conformance"),
            new("ANALYZE-QUICK_FACTS", "Quick facts dashboard"),
            new("ANALYZE-CLSR_HEALTH_MAP", "Health map CLSR × Zona"),
            new("ANALYZE-TOP5", "Top 5 (exposure, gap, drift, systemic)"),
            new("ANALYZE-TREND_ZONE", "Tren bulanan & skor per zona"),
            new("ANALYZE-IMPROVEMENT_INITIATIVES", "Inisiatif perbaikan"),
            new("CONFIG-DASHBOARD_TEXT", "Teks naratif dashboard"),
        };

        public static readonly IReadOnlySet<string> RequiredSheetNames =
            RequiredSheets.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        /// <summary>Sheet yang punya halaman kurasi khusus di Vue; sisanya memakai viewer generik.</summary>
        public static readonly IReadOnlyDictionary<string, CuratedSheet> Curated =
            new Dictionary<string, CuratedSheet>(StringComparer.Ordinal)
            {
                ["INPUT-SIF_Questions"] = new("/master/sif-questions", "SIF Questions", "checklist"),
                ["INPUT-Error_Traps"] = new("/master/error-traps", "Error Traps", "warning"),
                ["INPUT-HP_Tools"] = new("/master/hp-tools", "HP Tools", "gear"),
                ["INPUT-Drift_Conditions"] = new("/master/drift-conditions", "Drift Conditions", "refresh"),
                ["INPUT-Latent_Conditions"] = new("/master/latent-conditions", "Latent Conditions", "layers"),
                ["DATABASE_PSEC_CCVC"] = new("/master/ccvc-library", "PSEC & CCVC Library", "book"),
                ["ANALYZE-CONFORMANCE_SCORE"] = new("/master/observations", "Observations", "clipboard"),
                ["ANALYZE-IMPROVEMENT_INITIATIVES"] = new("/master/initiatives", "Improvement Initiatives", "target"),
            };

        /// <summary>Urutan grup di sidebar. Sheet di dalam grup mengikuti urutan aslinya di Excel.</summary>
        public static readonly IReadOnlyList<string> GroupOrder = new[]
        {
            "Data Input", "Database", "Analisis", "Konfigurasi", "Sumber", "Audit", "Helper", "Lainnya"
        };

        private static readonly Dictionary<string, string> GroupIcons = new(StringComparer.Ordinal)
        {
            ["Data Input"] = "clipboard",
            ["Database"] = "book",
            ["Analisis"] = "gear",
            ["Konfigurasi"] = "gear",
            ["Sumber"] = "file",
            ["Audit"] = "shield",
            ["Helper"] = "layers",
            ["Lainnya"] = "file",
        };

        public static IReadOnlyList<SheetDefinition> FindMissingSheets(IEnumerable<string> sheetNames)
        {
            var present = sheetNames.ToHashSet(StringComparer.Ordinal);
            return RequiredSheets.Where(s => !present.Contains(s.Name)).ToList();
        }

        public static string GroupOf(string name)
        {
            if (name.StartsWith("INPUT", StringComparison.OrdinalIgnoreCase)) return "Data Input";
            if (name.StartsWith("DATABASE", StringComparison.OrdinalIgnoreCase)) return "Database";
            if (name.StartsWith("ANALYZE", StringComparison.OrdinalIgnoreCase)) return "Analisis";
            if (name.StartsWith("CONFIG", StringComparison.OrdinalIgnoreCase)) return "Konfigurasi";
            if (name.StartsWith("SOURCE", StringComparison.OrdinalIgnoreCase)) return "Sumber";
            if (name.StartsWith("AUDIT", StringComparison.OrdinalIgnoreCase)) return "Audit";
            if (name.StartsWith("Helper", StringComparison.OrdinalIgnoreCase)) return "Helper";
            return "Lainnya";
        }

        public static string IconForGroup(string group) =>
            GroupIcons.TryGetValue(group, out var icon) ? icon : "file";

        public static string Slugify(string name)
        {
            var lowered = name.ToLowerInvariant();
            var slug = Regex.Replace(lowered, "[^a-z0-9]+", "-");
            return slug.Trim('-');
        }

        /// <summary>
        /// Label ringkas untuk sheet non-kurasi: token kategori di depan dibuang
        /// (sudah menjadi judul grup), lalu "_" / "-" diubah menjadi spasi.
        /// </summary>
        public static string ShortLabel(string name)
        {
            var stripped = Regex.Replace(name, "^(INPUT|DATABASE|ANALYZE|CONFIG|SOURCE|AUDIT|Helper)[-_]?", string.Empty,
                RegexOptions.IgnoreCase);

            var words = Regex.Replace(string.IsNullOrEmpty(stripped) ? name : stripped, "[_-]+", " ").Trim();
            return string.IsNullOrEmpty(words) ? name : words;
        }

        /// <summary>Mengubah indeks kolom 0-based menjadi huruf kolom Excel (0 → A, 26 → AA).</summary>
        public static string ColumnLetter(int index)
        {
            var sb = new StringBuilder();
            var n = index;
            do
            {
                sb.Insert(0, (char)('A' + (n % 26)));
                n = (n / 26) - 1;
            } while (n >= 0);

            return sb.ToString();
        }
    }
}
