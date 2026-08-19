using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data
{
    /// <summary>
    /// DbContext tunggal aplikasi. Semua pemetaan kolom ditulis lewat Fluent API
    /// di file konfigurasi terpisah (Data/Configurations) agar entity tetap bersih
    /// dari atribut persistensi.
    /// </summary>
    public class SifpDbContext : DbContext
    {
        private readonly ICurrentUserAccessor? _currentUser;

        public SifpDbContext(DbContextOptions<SifpDbContext> options, ICurrentUserAccessor? currentUser = null)
            : base(options)
        {
            _currentUser = currentUser;
        }

        // ---------- Master data observasi ----------
        public DbSet<Observation> Observations => Set<Observation>();
        public DbSet<SifQuestion> SifQuestions => Set<SifQuestion>();
        public DbSet<CcvcLibraryItem> CcvcLibraryItems => Set<CcvcLibraryItem>();
        public DbSet<ErrorTrap> ErrorTraps => Set<ErrorTrap>();
        public DbSet<HpTool> HpTools => Set<HpTool>();
        public DbSet<DriftCondition> DriftConditions => Set<DriftCondition>();
        public DbSet<LatentCondition> LatentConditions => Set<LatentCondition>();
        public DbSet<ImprovementInitiative> ImprovementInitiatives => Set<ImprovementInitiative>();

        // ---------- Agregat dashboard ----------
        public DbSet<ExecutiveMeasure> ExecutiveMeasures => Set<ExecutiveMeasure>();
        public DbSet<QuickFact> QuickFacts => Set<QuickFact>();
        public DbSet<ClsrHealthMapRow> ClsrHealthMapRows => Set<ClsrHealthMapRow>();
        public DbSet<TopFiveItem> TopFiveItems => Set<TopFiveItem>();
        public DbSet<TrendPoint> TrendPoints => Set<TrendPoint>();
        public DbSet<ZonaScore> ZonaScores => Set<ZonaScore>();
        public DbSet<DashboardText> DashboardTexts => Set<DashboardText>();

        // ---------- Import & worksheet mentah ----------
        public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
        public DbSet<Worksheet> Worksheets => Set<Worksheet>();
        public DbSet<WorksheetRow> WorksheetRows => Set<WorksheetRow>();

        // ---------- Keamanan & Maintenance ----------
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SifpDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        /// <summary>
        /// Mengisi CreatedAt/CreatedBy dan UpdatedAt/UpdatedBy secara terpusat.
        /// UTC dipakai supaya nilai tidak bergeser saat server dan klien beda zona waktu.
        /// </summary>
        private void ApplyAuditInformation()
        {
            var now = DateTime.UtcNow;
            var actor = _currentUser?.UserName ?? "SYSTEM";

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy ??= actor;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // CreatedAt/CreatedBy tidak boleh berubah saat update.
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;

                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = actor;
                }
            }
        }
    }

    /// <summary>
    /// Abstraksi tipis untuk mengetahui user yang sedang aktif tanpa membuat
    /// DbContext bergantung langsung pada HttpContext.
    /// </summary>
    public interface ICurrentUserAccessor
    {
        string UserName { get; }
        int? UserId { get; }
    }
}
