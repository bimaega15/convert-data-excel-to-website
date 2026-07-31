namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Kondisi drift yang teramati (sheet INPUT-Drift_Conditions).</summary>
    public class DriftCondition : AuditableEntity, IObservationChild
    {
        public int Id { get; set; }

        public int ObservationId { get; set; }
        public Observation? Observation { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }

        public string? Situation { get; set; }

        /// <summary>Klasifikasi tingkat 1, mis. "Skill-Based Performance Drift".</summary>
        public string? Level1 { get; set; }

        /// <summary>Kode klasifikasi, mis. "Drift.3".</summary>
        public string? Code { get; set; }

        /// <summary>Klasifikasi tingkat 2, mis. "Overreliance on experience".</summary>
        public string? Level2 { get; set; }

        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; } = true;

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
