namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Inisiatif perbaikan (sheet ANALYZE-IMPROVEMENT_INITIATIVES).</summary>
    public class ImprovementInitiative : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Improvement_ID dari Excel, mis. "IMP-R4-001". Unik.</summary>
        public string ImprovementCode { get; set; } = string.Empty;

        public string? Initiative { get; set; }
        public string? RelatedClsr { get; set; }

        /// <summary>Kolom V&amp;V_Team_Asset_Owner.</summary>
        public string? Owner { get; set; }

        public string? Status { get; set; }

        /// <summary>Progress dalam persen bulat (0-100).</summary>
        public int ProgressPercent { get; set; }

        public string? ExpectedImpact { get; set; }
        public string? Notes { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
