namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Teks naratif dashboard (sheet CONFIG-DASHBOARD_TEXT), mis. "Top SIF Exposure Note".
    /// Editable lewat halaman admin tanpa perlu import ulang Excel.
    /// </summary>
    public class DashboardText : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>Kolom Section di Excel. Unik.</summary>
        public string Section { get; set; } = string.Empty;

        public string? Text { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
