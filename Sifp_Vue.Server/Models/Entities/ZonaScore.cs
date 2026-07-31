namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Skor conformance per zona (sheet ANALYZE-TREND_ZONE, sisi kanan).</summary>
    public class ZonaScore : AuditableEntity
    {
        public int Id { get; set; }

        public int Zona { get; set; }

        /// <summary>Label siap tampil, mis. "Zona 11".</summary>
        public string? ZonaLabel { get; set; }

        /// <summary>Skor dalam persen (0-100).</summary>
        public decimal ScorePercent { get; set; }

        public int ObservationCount { get; set; }
        public int DisplayOrder { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
