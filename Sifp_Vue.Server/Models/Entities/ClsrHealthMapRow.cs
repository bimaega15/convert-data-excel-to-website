namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Satu baris health map CLSR × Zona (sheet ANALYZE-CLSR_HEALTH_MAP).
    /// Zona 11-14 disimpan sebagai kolom tetap karena strukturnya memang tetap di workbook.
    /// </summary>
    public class ClsrHealthMapRow : AuditableEntity
    {
        public int Id { get; set; }

        public string ClsrId { get; set; } = string.Empty;
        public string? ClsrDescription { get; set; }

        public string? Zona11Status { get; set; }
        public decimal? Zona11Score { get; set; }
        public string? Zona12Status { get; set; }
        public decimal? Zona12Score { get; set; }
        public string? Zona13Status { get; set; }
        public decimal? Zona13Score { get; set; }
        public string? Zona14Status { get; set; }
        public decimal? Zona14Score { get; set; }

        public decimal? Regional4Score { get; set; }
        public string? HealthStatus { get; set; }

        public int DisplayOrder { get; set; }

        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }
    }
}
