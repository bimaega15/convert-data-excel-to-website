namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Titik tren conformance bulanan (sheet ANALYZE-TREND_ZONE, sisi kiri).
    /// Baris tanpa observasi diperlakukan sebagai proyeksi, bukan aktual.
    /// </summary>
    public class TrendPoint : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Tanggal awal bulan, mis. 2026-05-01.</summary>
        public DateOnly PeriodMonth { get; set; }

        /// <summary>Label siap tampil, mis. "May-26".</summary>
        public string? MonthLabel { get; set; }

        /// <summary>Skor aktual dalam persen (0-100). Null bila bulan belum terealisasi.</summary>
        public decimal? ActualPercent { get; set; }

        /// <summary>Skor rencana dalam persen (0-100).</summary>
        public decimal? PlannedPercent { get; set; }

        public int ObservationCount { get; set; }

        /// <summary>True bila baris ini garis proyeksi, bukan realisasi.</summary>
        public bool IsProjection { get; set; }

        public int DisplayOrder { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
