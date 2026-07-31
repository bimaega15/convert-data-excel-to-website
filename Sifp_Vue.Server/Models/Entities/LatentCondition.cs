namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Kondisi laten yang teridentifikasi (sheet INPUT-Latent_Conditions).</summary>
    public class LatentCondition : AuditableEntity, IObservationChild
    {
        public int Id { get; set; }

        public int ObservationId { get; set; }
        public Observation? Observation { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }

        public string? ObservationText { get; set; }

        /// <summary>Klasifikasi tingkat 1, mis. "Competency / Training".</summary>
        public string? Level1 { get; set; }

        /// <summary>Kode klasifikasi, mis. "Latent.1".</summary>
        public string? Code { get; set; }

        /// <summary>Klasifikasi tingkat 2, mis. "Inadequate WAH understanding".</summary>
        public string? Level2 { get; set; }

        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; } = true;

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
