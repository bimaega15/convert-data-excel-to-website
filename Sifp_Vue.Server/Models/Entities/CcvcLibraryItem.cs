namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Master library PSEC &amp; CCVC (sheet DATABASE_PSEC_CCVC).
    /// Referensi statis, tidak terikat pada satu observasi.
    /// </summary>
    public class CcvcLibraryItem : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Nomor urut asli di Excel (kolom No).</summary>
        public int? RowNo { get; set; }

        public string? ProtocolGroup { get; set; }
        public string? PsecId { get; set; }
        public string? PsecName { get; set; }
        public string? ExposureType { get; set; }

        /// <summary>Kunci bisnis, mis. "CLSR01-A". Unik.</summary>
        public string CcvcId { get; set; } = string.Empty;

        public string? QuestionCode { get; set; }
        public string? QuestionSummary { get; set; }
        public string? VerificationPurpose { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
