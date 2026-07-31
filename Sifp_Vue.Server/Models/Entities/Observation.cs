namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Satu baris observasi V&amp;V (sheet ANALYZE-CONFORMANCE_SCORE).
    /// Menjadi induk dari SIF question, error trap, HP tool, drift, dan latent condition.
    /// </summary>
    public class Observation : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Obs_ID dari Excel, mis. "OBS-001". Unik dan dipakai sebagai kunci bisnis.</summary>
        public string ObsCode { get; set; } = string.Empty;

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public DateOnly? ObservationDate { get; set; }
        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? AreaEquipment { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }

        public string? Observer1 { get; set; }
        public string? Observer2 { get; set; }
        public string? Observer3 { get; set; }

        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int NaCount { get; set; }

        /// <summary>Skor conformance dalam persen (0-100), mis. 44.44.</summary>
        public decimal? PerformancePercent { get; set; }

        public int? ObservationSequence { get; set; }

        /// <summary>Kolom PSIE_Eligible di Excel berisi "Y"/"N".</summary>
        public bool PsieEligible { get; set; }

        /// <summary>Observation_Status, mis. "Baseline".</summary>
        public string? Status { get; set; }

        /// <summary>Active_Observation ("Y"/"N").</summary>
        public bool IsActive { get; set; } = true;

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }

        public ICollection<SifQuestion> SifQuestions { get; set; } = new List<SifQuestion>();
        public ICollection<ErrorTrap> ErrorTraps { get; set; } = new List<ErrorTrap>();
        public ICollection<HpTool> HpTools { get; set; } = new List<HpTool>();
        public ICollection<DriftCondition> DriftConditions { get; set; } = new List<DriftCondition>();
        public ICollection<LatentCondition> LatentConditions { get; set; } = new List<LatentCondition>();
    }
}
