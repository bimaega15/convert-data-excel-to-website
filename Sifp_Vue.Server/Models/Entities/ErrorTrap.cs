namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Error trap yang teridentifikasi pada sebuah observasi (sheet INPUT-Error_Traps).</summary>
    public class ErrorTrap : AuditableEntity, IObservationChild
    {
        public int Id { get; set; }

        public int ObservationId { get; set; }
        public Observation? Observation { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }

        /// <summary>Kategori error trap, mis. "Individual_Factor".</summary>
        public string? Category { get; set; }

        public string? TrapName { get; set; }
        public string? Comments { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
