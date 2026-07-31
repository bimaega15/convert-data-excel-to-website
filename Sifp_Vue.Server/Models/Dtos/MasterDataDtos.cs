using System.ComponentModel.DataAnnotations;

namespace Sifp_Vue.Server.Models.Dtos
{
    // Nama field sengaja dibuat identik dengan hasil konversi Excel di
    // sifp_vue.client/src/data/generated/*.json, sehingga halaman Vue bisa
    // berpindah dari JSON statis ke API tanpa mengubah template.

    public class ObservationDto
    {
        /// <summary>Obs_ID, mis. "OBS-001".</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Primary key database, dipakai halaman admin untuk edit/hapus.</summary>
        public int Key { get; set; }

        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }

        /// <summary>Format ISO "yyyy-MM-dd" agar konsisten dengan converter Excel.</summary>
        public string? Date { get; set; }

        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? Area { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }
        public IReadOnlyList<string> Observers { get; set; } = Array.Empty<string>();
        public int Yes { get; set; }
        public int No { get; set; }
        public int Na { get; set; }
        public decimal? Performance { get; set; }
        public int? Sequence { get; set; }

        /// <summary>"Y" atau "N", mengikuti bentuk asli di Excel.</summary>
        public string PsieEligible { get; set; } = "N";

        public string? Status { get; set; }
        public string Active { get; set; } = "Y";
    }

    public class ObservationRequest
    {
        [Required(ErrorMessage = "Obs ID wajib diisi.")]
        [StringLength(50)]
        public string ObsCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ProtocolCode { get; set; }

        [StringLength(200)]
        public string? ProtocolName { get; set; }

        public DateOnly? ObservationDate { get; set; }

        [Range(1, 99, ErrorMessage = "Zona harus antara 1 dan 99.")]
        public int? Zona { get; set; }

        [StringLength(200)] public string? Site { get; set; }
        [StringLength(200)] public string? AreaEquipment { get; set; }
        [StringLength(300)] public string? Activity { get; set; }
        [StringLength(200)] public string? Company { get; set; }
        [StringLength(150)] public string? Observer1 { get; set; }
        [StringLength(150)] public string? Observer2 { get; set; }
        [StringLength(150)] public string? Observer3 { get; set; }

        [Range(0, int.MaxValue)] public int YesCount { get; set; }
        [Range(0, int.MaxValue)] public int NoCount { get; set; }
        [Range(0, int.MaxValue)] public int NaCount { get; set; }

        [Range(0, 100, ErrorMessage = "Performance harus antara 0 dan 100.")]
        public decimal? PerformancePercent { get; set; }

        public int? ObservationSequence { get; set; }
        public bool PsieEligible { get; set; }

        [StringLength(50)] public string? Status { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SifQuestionDto
    {
        public int Key { get; set; }
        public string ObsId { get; set; } = string.Empty;
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? QuestionRef { get; set; }
        public string? CcvcId { get; set; }
        public string? Question { get; set; }
        public string Answer { get; set; } = "-";
        public string? Comments { get; set; }
        public string? SifExposure { get; set; }
        public string? CriticalSafeguard { get; set; }
        public string? Date { get; set; }
        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }
    }

    public class CcvcLibraryItemDto
    {
        public int Key { get; set; }
        public int? No { get; set; }
        public string? ProtocolGroup { get; set; }
        public string? PsecId { get; set; }
        public string? PsecName { get; set; }
        public string? ExposureType { get; set; }
        public string CcvcId { get; set; } = string.Empty;
        public string? QuestionCode { get; set; }
        public string? QuestionSummary { get; set; }
        public string? VerificationPurpose { get; set; }
    }

    public class ErrorTrapDto
    {
        public int Key { get; set; }
        public string ObsId { get; set; } = string.Empty;
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Category { get; set; }
        public string? ErrorTrap { get; set; }
        public string? Comments { get; set; }
    }

    public class HpToolDto
    {
        public int Key { get; set; }
        public string ObsId { get; set; } = string.Empty;
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Tool { get; set; }
        public string? Tujuan { get; set; }
        public string? KapanDigunakan { get; set; }
        public string? CaraPakai { get; set; }
        public string? EffectivenessNotes { get; set; }
    }

    public class DriftConditionDto
    {
        public int Key { get; set; }
        public string ObsId { get; set; } = string.Empty;
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Situation { get; set; }
        public string? Level1 { get; set; }
        public string? Code { get; set; }
        public string? Level2 { get; set; }
        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public string Active { get; set; } = "Y";
    }

    public class LatentConditionDto
    {
        public int Key { get; set; }
        public string ObsId { get; set; } = string.Empty;
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Observation { get; set; }
        public string? Level1 { get; set; }
        public string? Code { get; set; }
        public string? Level2 { get; set; }
        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public string Active { get; set; } = "Y";
    }

    public class InitiativeDto
    {
        /// <summary>Improvement_ID, mis. "IMP-R4-001".</summary>
        public string Id { get; set; } = string.Empty;

        public int Key { get; set; }
        public string? Initiative { get; set; }
        public string? RelatedClsr { get; set; }
        public string? Owner { get; set; }
        public string? Status { get; set; }
        public int Progress { get; set; }
        public string? ExpectedImpact { get; set; }
        public string? Notes { get; set; }
    }

    public class InitiativeRequest
    {
        [Required(ErrorMessage = "Improvement ID wajib diisi.")]
        [StringLength(50)]
        public string ImprovementCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama inisiatif wajib diisi.")]
        [StringLength(300)]
        public string? Initiative { get; set; }

        [StringLength(200)] public string? RelatedClsr { get; set; }
        [StringLength(150)] public string? Owner { get; set; }
        [StringLength(50)] public string? Status { get; set; }

        [Range(0, 100, ErrorMessage = "Progress harus antara 0 dan 100.")]
        public int ProgressPercent { get; set; }

        public string? ExpectedImpact { get; set; }
        public string? Notes { get; set; }
    }
}
