namespace Sifp_Vue.Server.Data.Seeders
{
    /// <summary>
    /// Satu unit seeding. Seluruh implementasi dijalankan berurutan oleh
    /// <see cref="DatabaseSeeder"/> dan wajib idempoten — dijalankan berkali-kali
    /// tidak boleh menggandakan data.
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>Urutan eksekusi; angka lebih kecil dijalankan lebih dulu.</summary>
        int Order { get; }

        string Name { get; }

        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
