namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// KPI eksekutif PSEC / CCVC / PSIE / CONF (sheet ANALYZE-EXECUTIVE_MEASURES).
    /// Menjadi sumber kartu KPI dan gauge conformance di dashboard.
    /// </summary>
    public class ExecutiveMeasure : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Metric_Code: PSEC, CCVC, PSIE, atau CONF. Unik.</summary>
        public string MetricCode { get; set; } = string.Empty;

        public string? MetricName { get; set; }
        public decimal? Numerator { get; set; }
        public decimal? Denominator { get; set; }

        /// <summary>Score_% dalam persen (0-100).</summary>
        public decimal? ScorePercent { get; set; }

        /// <summary>Target_% dalam persen (0-100).</summary>
        public decimal? TargetPercent { get; set; }

        public string? Status { get; set; }
        public string? Notes { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
