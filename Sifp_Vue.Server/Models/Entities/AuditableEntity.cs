namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Kolom audit yang dipakai seluruh tabel. Diisi otomatis oleh
    /// <see cref="Data.SifpDbContext.SaveChangesAsync(CancellationToken)"/>,
    /// jadi service/repository tidak perlu mengisinya manual.
    /// </summary>
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
