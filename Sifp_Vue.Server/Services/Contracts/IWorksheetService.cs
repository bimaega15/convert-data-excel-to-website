using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Services.Contracts
{
    public interface IWorksheetService
    {
        /// <summary>
        /// Manifest worksheet dari batch import terakhir yang berhasil.
        /// Menjadi sumber menu sidebar aplikasi Vue.
        /// </summary>
        Task<WorksheetManifestDto> GetManifestAsync(CancellationToken cancellationToken = default);

        /// <summary>Data mentah satu worksheet untuk viewer generik. Null bila slug tidak ada.</summary>
        Task<WorksheetDataDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    }
}
