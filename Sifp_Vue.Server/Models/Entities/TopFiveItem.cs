namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Baris panel "Top 5" (sheet ANALYZE-TOP5). Satu tabel menampung empat kategori:
    /// Top SIF Exposure, Top Critical Safeguard Gap, Top Recurring Drift, Top Systemic Issue.
    /// </summary>
    public class TopFiveItem : AuditableEntity
    {
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;
        public string? Item { get; set; }
        public int Count { get; set; }

        /// <summary>Rasio 0-1 seperti di Excel (bukan persen).</summary>
        public decimal? Percent { get; set; }

        public int? Denominator { get; set; }
        public int DisplayOrder { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
