using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    public interface IDashboardService
    {
        /// <summary>Menyusun payload dashboard lengkap untuk aplikasi Vue.</summary>
        Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

        /// <summary>Ringkasan angka untuk kartu di halaman /admin (Razor).</summary>
        Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(CancellationToken cancellationToken = default);
    }
}
