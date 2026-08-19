using Microsoft.EntityFrameworkCore;

namespace Sifp_Vue.Server.Data.Seeders
{
    public class SeedOptions
    {
        public const string SectionName = "Seed";

        /// <summary>Jalankan migration otomatis saat aplikasi start.</summary>
        public bool AutoMigrate { get; set; } = true;

        /// <summary>Jalankan seeder saat aplikasi start.</summary>
        public bool RunSeeders { get; set; } = true;

        /// <summary>Ikut mengisi master data contoh dari hasil konversi Excel.</summary>
        public bool SeedSampleData { get; set; } = true;

        /// <summary>Lokasi folder <c>src/data/generated</c> milik proyek Vue, relatif terhadap content root.</summary>
        public string GeneratedDataPath { get; set; } = "../sifp_vue.client/src/data/generated";

        public string AdminUsername { get; set; } = "admin";
        public string AdminEmail { get; set; } = "admin@pertamina.com";
        public string AdminFullName { get; set; } = "Administrator";

        /// <summary>
        /// Password admin awal. WAJIB diganti lewat user-secrets / environment variable
        /// sebelum dipakai di luar mesin developer.
        /// </summary>
        public string AdminPassword { get; set; } = DefaultAdminPassword;

        public const string DefaultAdminPassword = "Admin#12345";
    }

    /// <summary>
    /// Titik masuk tunggal untuk migration + seeding. Dipanggil sekali dari Program.cs
    /// pada scope-nya sendiri, sehingga DbContext-nya tidak tercampur dengan request.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly SifpDbContext _context;
        private readonly IEnumerable<IDataSeeder> _seeders;
        private readonly SeedOptions _options;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            SifpDbContext context,
            IEnumerable<IDataSeeder> seeders,
            Microsoft.Extensions.Options.IOptions<SeedOptions> options,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _seeders = seeders;
            _options = options.Value;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (_options.AutoMigrate)
            {
                var pending = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                if (pending.Count > 0)
                {
                    _logger.LogInformation("Menjalankan {Count} migration: {Names}", pending.Count, string.Join(", ", pending));
                    await _context.Database.MigrateAsync(cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Database sudah pada versi migration terbaru.");
                }
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
                    BEGIN
                        CREATE TABLE [dbo].[AuditLogs] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Timestamp] DATETIME2 NOT NULL,
                            [Username] NVARCHAR(150) NOT NULL,
                            [Action] NVARCHAR(200) NOT NULL,
                            [Module] NVARCHAR(100) NOT NULL,
                            [Details] NVARCHAR(MAX) NULL,
                            [IpAddress] NVARCHAR(50) NULL,
                            [StatusCode] INT NULL
                        );
                    END
                ", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Penyiapan tabel AuditLogs: {Message}", ex.Message);
            }

            if (!_options.RunSeeders)
            {
                _logger.LogInformation("Seeder dilewati (Seed:RunSeeders = false).");
                return;
            }

            foreach (var seeder in _seeders.OrderBy(s => s.Order))
            {
                try
                {
                    await seeder.SeedAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Seeder yang gagal tidak boleh menggagalkan startup aplikasi:
                    // API dan halaman admin tetap harus bisa diakses untuk diagnosis.
                    _logger.LogError(ex, "Seeder {Name} gagal", seeder.Name);
                }
            }
        }
    }
}
