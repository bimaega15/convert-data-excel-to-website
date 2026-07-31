namespace Sifp_Vue.Server.Models.Entities
{
    public enum ImportStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }

    /// <summary>
    /// Jejak satu kali import workbook. Semua baris master data menyimpan
    /// <c>ImportBatchId</c> sehingga selalu bisa ditelusuri berasal dari file mana.
    /// </summary>
    public class ImportBatch : AuditableEntity
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }

        /// <summary>SHA-256 file asli, dipakai mendeteksi import berkas yang sama.</summary>
        public string? FileHash { get; set; }

        public ImportStatus Status { get; set; } = ImportStatus.Pending;

        public int SheetCount { get; set; }
        public int TotalRows { get; set; }

        /// <summary>Jumlah sel yang diubah user di layar preview sebelum submit.</summary>
        public int EditCount { get; set; }

        /// <summary>Daftar edit sel (JSON) persis seperti dikirim klien, untuk audit.</summary>
        public string? EditsJson { get; set; }

        /// <summary>Ringkasan hasil parse (JSON) untuk audit.</summary>
        public string? SummaryJson { get; set; }

        public string? ErrorMessage { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    }
}
