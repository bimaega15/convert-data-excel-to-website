using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    public class InitiativeService : IInitiativeService
    {
        private readonly IInitiativeRepository _repository;
        private readonly ILogger<InitiativeService> _logger;

        public InitiativeService(IInitiativeRepository repository, ILogger<InitiativeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<PagedResult<InitiativeDto>> GetPagedAsync(InitiativeQuery query, CancellationToken cancellationToken = default)
            => _repository.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public async Task<IReadOnlyList<InitiativeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.Query().OrderBy(x => x.ImprovementCode).ToListAsync(cancellationToken);
            return items.Select(x => x.ToDto()).ToList();
        }

        public async Task<InitiativeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<ApiResponse<InitiativeDto>> CreateAsync(InitiativeRequest request, CancellationToken cancellationToken = default)
        {
            if (await _repository.CodeExistsAsync(request.ImprovementCode.Trim(), null, cancellationToken))
            {
                return ApiResponse<InitiativeDto>.Fail($"Improvement ID \"{request.ImprovementCode}\" sudah dipakai.");
            }

            var entity = new ImprovementInitiative();
            request.ApplyTo(entity);

            await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inisiatif {Code} dibuat (Id={Id})", entity.ImprovementCode, entity.Id);
            return ApiResponse<InitiativeDto>.Ok(entity.ToDto(), "Inisiatif berhasil disimpan.");
        }

        public async Task<ApiResponse<InitiativeDto>> UpdateAsync(int id, InitiativeRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity is null)
            {
                return ApiResponse<InitiativeDto>.Fail("Inisiatif tidak ditemukan.");
            }

            if (await _repository.CodeExistsAsync(request.ImprovementCode.Trim(), id, cancellationToken))
            {
                return ApiResponse<InitiativeDto>.Fail($"Improvement ID \"{request.ImprovementCode}\" sudah dipakai.");
            }

            request.ApplyTo(entity);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inisiatif {Code} diperbarui (Id={Id})", entity.ImprovementCode, entity.Id);
            return ApiResponse<InitiativeDto>.Ok(entity.ToDto(), "Inisiatif berhasil diperbarui.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var deleted = await _repository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return ApiResponse<bool>.Fail("Inisiatif tidak ditemukan.");
            }

            await _repository.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.Ok(true, "Inisiatif berhasil dihapus.");
        }
    }
}
