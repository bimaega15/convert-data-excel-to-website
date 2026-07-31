namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Metadata satu worksheet Excel. Menjadi sumber tunggal menu sidebar Vue
    /// (menggantikan peran <c>src/data/generated/sheets/_manifest.json</c>),
    /// sehingga jumlah menu otomatis mengikuti isi workbook.
    /// </summary>
    public class Worksheet : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Nama sheet asli di Excel, mis. "ANALYZE-CONFORMANCE_SCORE".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Slug URL, mis. "analyze-conformance-score". Unik per batch.</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>Urutan sheet di workbook.</summary>
        public int SheetIndex { get; set; }

        /// <summary>Grup sidebar: Data Input / Database / Analisis / Konfigurasi / dst.</summary>
        public string? GroupName { get; set; }

        public string? Label { get; set; }
        public string? Icon { get; set; }

        /// <summary>Route Vue. Sheet kurasi menunjuk halaman khusus, sisanya /sheet/{slug}.</summary>
        public string? Route { get; set; }

        /// <summary>True bila sheet punya halaman kurasi sendiri di Vue.</summary>
        public bool IsCurated { get; set; }

        /// <summary>True bila sheet termasuk daftar sheet wajib (REQUIRED_SHEETS).</summary>
        public bool IsRequired { get; set; }

        public int RowCount { get; set; }
        public int ColCount { get; set; }

        public int ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }

        public ICollection<WorksheetRow> Rows { get; set; } = new List<WorksheetRow>();
    }
}
