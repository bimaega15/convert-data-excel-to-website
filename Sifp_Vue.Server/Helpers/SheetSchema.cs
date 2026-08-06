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
        // Nama sheet mengikuti template resmi (SifpAssurance_Template.xlsm) yang wajib
        // diupload. Harus identik dengan sifp_vue.client/src/data/sheet-schema.js.
        public static readonly IReadOnlyList<SheetDefinition> RequiredSheets = new List<SheetDefinition>
        {
            new("SIF Questions", "Jawaban pertanyaan verifikasi SIF"),
            new("Error Traps", "Error traps per observasi"),
            new("HP Tools", "Human Performance Tools"),
            new("Drift Conditions", "Kondisi drift"),
            new("Latent Conditions", "Kondisi laten"),
            new("PSEC CCVC", "Master library PSEC & CCVC"),
            new("Conformance Score", "Rekap observasi & skor"),
            new("Executive Measures", "KPI PSEC / CCVC / PSIE / Conformance"),
            new("Quick Facts", "Quick facts dashboard"),
            new("CLSR Health", "Health map CLSR × Zona"),
            new("Top 5", "Top 5 (exposure, gap, drift, systemic)"),
            new("Trend Zone", "Tren bulanan & skor per zona"),
            new("Improvement Initiatives", "Inisiatif perbaikan"),
            new("Dashboard Text", "Teks naratif dashboard"),
        };

        public static readonly IReadOnlySet<string> RequiredSheetNames =
            RequiredSheets.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        /// <summary>Sheet yang punya halaman kurasi khusus di Vue; sisanya memakai viewer generik.</summary>
        public static readonly IReadOnlyDictionary<string, CuratedSheet> Curated =
            new Dictionary<string, CuratedSheet>(StringComparer.Ordinal)
            {
                ["SIF Questions"] = new("/master/sif-questions", "SIF Questions", "checklist"),
                ["Error Traps"] = new("/master/error-traps", "Error Traps", "warning"),
                ["HP Tools"] = new("/master/hp-tools", "HP Tools", "gear"),
                ["Drift Conditions"] = new("/master/drift-conditions", "Drift Conditions", "refresh"),
                ["Latent Conditions"] = new("/master/latent-conditions", "Latent Conditions", "layers"),
                ["PSEC CCVC"] = new("/master/ccvc-library", "PSEC & CCVC Library", "book"),
                ["Conformance Score"] = new("/master/observations", "Observations", "clipboard"),
                ["Improvement Initiatives"] = new("/master/initiatives", "Improvement Initiatives", "target"),
            };

        /// <summary>
        /// Grup sidebar per sheet. Nama template tidak lagi berawalan kategori
        /// (INPUT-/ANALYZE-/…), jadi grup dipetakan eksplisit di sini.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> SheetGroups =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SIF Questions"] = "Data Input",
                ["Error Traps"] = "Data Input",
                ["HP Tools"] = "Data Input",
                ["Drift Conditions"] = "Data Input",
                ["Latent Conditions"] = "Data Input",
                ["PSEC CCVC"] = "Database",
                ["Conformance Score"] = "Analisis",
                ["Executive Measures"] = "Analisis",
                ["Quick Facts"] = "Analisis",
                ["CLSR Health"] = "Analisis",
                ["Top 5"] = "Analisis",
                ["Trend Zone"] = "Analisis",
                ["Improvement Initiatives"] = "Analisis",
                ["Dashboard Text"] = "Konfigurasi",
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

        public static string GroupOf(string name) =>
            SheetGroups.TryGetValue(name, out var group) ? group : "Lainnya";

        public static string IconForGroup(string group) =>
            GroupIcons.TryGetValue(group, out var icon) ? icon : "file";

        public static string Slugify(string name)
        {
            var lowered = name.ToLowerInvariant();
            var slug = Regex.Replace(lowered, "[^a-z0-9]+", "-");
            return slug.Trim('-');
        }

        /// <summary>
        /// Label ringkas untuk sheet non-kurasi. Nama template sudah rapi (mis.
        /// "Executive Measures"), jadi cukup normalkan pemisah "_"/"-" menjadi spasi.
        /// </summary>
        public static string ShortLabel(string name)
        {
            var words = Regex.Replace(name, "[_-]+", " ").Trim();
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
