namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Jawaban pertanyaan verifikasi SIF per observasi (sheet INPUT-SIF_Questions).</summary>
    public class SifQuestion : AuditableEntity, IObservationChild
    {
        public int Id { get; set; }

        public int ObservationId { get; set; }
        public Observation? Observation { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? QuestionRef { get; set; }

        /// <summary>Referensi ke <see cref="CcvcLibraryItem.CcvcId"/>, mis. "CLSR07-A".</summary>
        public string? CcvcId { get; set; }

        public string? QuestionText { get; set; }

        /// <summary>Hasil jawaban: YES / NO / NA / "-".</summary>
        public string Answer { get; set; } = "-";

        public string? Comments { get; set; }
        public string? SifExposure { get; set; }
        public string? CriticalSafeguard { get; set; }

        // Denormalisasi dari observasi supaya filter & export tidak selalu perlu join.
        public DateOnly? ObservationDate { get; set; }
        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
