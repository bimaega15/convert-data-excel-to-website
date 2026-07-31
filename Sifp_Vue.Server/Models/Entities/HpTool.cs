namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Human Performance tool yang dipakai pada sebuah observasi (sheet INPUT-HP_Tools).</summary>
    public class HpTool : AuditableEntity, IObservationChild
    {
        public int Id { get; set; }

        public int ObservationId { get; set; }
        public Observation? Observation { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }

        public string? ToolName { get; set; }
        public string? Tujuan { get; set; }
        public string? KapanDigunakan { get; set; }
        public string? CaraPakai { get; set; }
        public string? EffectivenessNotes { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
