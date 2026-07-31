using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    public class ObservationService : IObservationService
    {
        private readonly IObservationRepository _repository;
        private readonly ILogger<ObservationService> _logger;

        public ObservationService(IObservationRepository repository, ILogger<ObservationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<PagedResult<ObservationDto>> GetPagedAsync(ObservationQuery query, CancellationToken cancellationToken = default)
            => _repository.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public async Task<IReadOnlyList<ObservationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.Query().OrderBy(x => x.ObsCode).ToListAsync(cancellationToken);
            return items.Select(x => x.ToDto()).ToList();
        }

        public async Task<ObservationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<ObservationDto?> GetByCodeAsync(string obsCode, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByObsCodeAsync(obsCode, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<ObservationDetailDto?> GetDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetWithDetailsAsync(id, cancellationToken);
            if (entity is null)
            {
                return null;
            }

            // Navigasi balik diisi manual: entity dimuat lewat Include dari sisi induk,
            // sehingga anak-anaknya belum punya referensi ke Observation untuk ObsCode.
            foreach (var child in entity.SifQuestions) child.Observation = entity;
            foreach (var child in entity.ErrorTraps) child.Observation = entity;
            foreach (var child in entity.HpTools) child.Observation = entity;
            foreach (var child in entity.DriftConditions) child.Observation = entity;
            foreach (var child in entity.LatentConditions) child.Observation = entity;

            return new ObservationDetailDto
            {
                Observation = entity.ToDto(),
                SifQuestions = entity.SifQuestions.Select(x => x.ToDto()).ToList(),
                ErrorTraps = entity.ErrorTraps.Select(x => x.ToDto()).ToList(),
                HpTools = entity.HpTools.Select(x => x.ToDto()).ToList(),
                DriftConditions = entity.DriftConditions.Select(x => x.ToDto()).ToList(),
                LatentConditions = entity.LatentConditions.Select(x => x.ToDto()).ToList()
            };
        }

        public async Task<ApiResponse<ObservationDto>> CreateAsync(ObservationRequest request, CancellationToken cancellationToken = default)
        {
            if (await _repository.ObsCodeExistsAsync(request.ObsCode.Trim(), null, cancellationToken))
            {
                return ApiResponse<ObservationDto>.Fail($"Obs ID \"{request.ObsCode}\" sudah dipakai observasi lain.");
            }

            var entity = new Observation();
            request.ApplyTo(entity);

            await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Observasi {ObsCode} dibuat (Id={Id})", entity.ObsCode, entity.Id);
            return ApiResponse<ObservationDto>.Ok(entity.ToDto(), "Observasi berhasil disimpan.");
        }

        public async Task<ApiResponse<ObservationDto>> UpdateAsync(int id, ObservationRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity is null)
            {
                return ApiResponse<ObservationDto>.Fail("Observasi tidak ditemukan.");
            }

            if (await _repository.ObsCodeExistsAsync(request.ObsCode.Trim(), id, cancellationToken))
            {
                return ApiResponse<ObservationDto>.Fail($"Obs ID \"{request.ObsCode}\" sudah dipakai observasi lain.");
            }

            request.ApplyTo(entity);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Observasi {ObsCode} diperbarui (Id={Id})", entity.ObsCode, entity.Id);
            return ApiResponse<ObservationDto>.Ok(entity.ToDto(), "Observasi berhasil diperbarui.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            // Data anak ikut terhapus lewat cascade yang dikonfigurasi di ObservationConfigurations.
            var deleted = await _repository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return ApiResponse<bool>.Fail("Observasi tidak ditemukan.");
            }

            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Observasi Id={Id} dihapus beserta seluruh data turunannya", id);

            return ApiResponse<bool>.Ok(true, "Observasi berhasil dihapus.");
        }

        public async Task<ObservationFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            return new ObservationFilterOptionsDto
            {
                Zonas = await query.Where(x => x.Zona != null).Select(x => x.Zona!.Value).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
                ProtocolCodes = await query.Where(x => x.ProtocolCode != null).Select(x => x.ProtocolCode!).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
                Sites = await query.Where(x => x.Site != null).Select(x => x.Site!).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
                Companies = await query.Where(x => x.Company != null).Select(x => x.Company!).Distinct().OrderBy(x => x).ToListAsync(cancellationToken),
                Statuses = await query.Where(x => x.Status != null).Select(x => x.Status!).Distinct().OrderBy(x => x).ToListAsync(cancellationToken)
            };
        }
    }
}
