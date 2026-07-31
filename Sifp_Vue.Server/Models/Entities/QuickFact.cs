namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Angka ringkas di panel Quick Facts dashboard (sheet ANALYZE-QUICK_FACTS).</summary>
    public class QuickFact : AuditableEntity
    {
        public int Id { get; set; }

        public string FactName { get; set; } = string.Empty;

        /// <summary>Nilai sudah dalam bentuk siap tampil, mis. "23" atau "78.26%".</summary>
        public string? FactValue { get; set; }

        /// <summary>Nama ikon yang dipakai komponen Vue DashIcon, mis. "clipboard".</summary>
        public string? Icon { get; set; }

        public int DisplayOrder { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
