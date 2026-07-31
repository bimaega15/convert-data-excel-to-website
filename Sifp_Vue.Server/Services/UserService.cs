using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository users,
            IRoleRepository roles,
            IPasswordHasher passwordHasher,
            ILogger<UserService> logger)
        {
            _users = users;
            _roles = roles;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public Task<PagedResult<UserDto>> GetPagedAsync(QueryParameters query, CancellationToken cancellationToken = default)
            => _users.Filter(query).ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetWithRolesAsync(id, cancellationToken);
            return user?.ToDto();
        }

        public Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
            => _roles.GetAllWithCountsAsync(cancellationToken);

        public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, string actor, CancellationToken cancellationToken = default)
        {
            var username = request.Username.Trim();

            if (await _users.UsernameExistsAsync(username, null, cancellationToken))
            {
                return ApiResponse<UserDto>.Fail($"Username \"{username}\" sudah dipakai.");
            }

            var user = new User
            {
                Username = username,
                Email = request.Email?.Trim(),
                FullName = request.FullName?.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Zona = request.Zona,
                IsActive = request.IsActive
            };

            await _users.AddAsync(user, cancellationToken);
            // Simpan dulu supaya user.Id terisi sebelum baris UserRoles dibuat.
            await _users.SaveChangesAsync(cancellationToken);

            await _users.ReplaceRolesAsync(user.Id, request.RoleIds, actor, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Username} dibuat oleh {Actor}", user.Username, actor);

            var created = await _users.GetWithRolesAsync(user.Id, cancellationToken);
            return ApiResponse<UserDto>.Ok(created!.ToDto(), "User berhasil dibuat.");
        }

        public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserRequest request, string actor, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(id, cancellationToken);
            if (user is null)
            {
                return ApiResponse<UserDto>.Fail("User tidak ditemukan.");
            }

            user.Email = request.Email?.Trim();
            user.FullName = request.FullName?.Trim();
            user.Zona = request.Zona;
            user.IsActive = request.IsActive;

            // Password hanya di-hash ulang bila field-nya benar-benar diisi.
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
                _logger.LogInformation("Password user {Username} direset oleh {Actor}", user.Username, actor);
            }

            await _users.ReplaceRolesAsync(user.Id, request.RoleIds, actor, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            var updated = await _users.GetWithRolesAsync(user.Id, cancellationToken);
            return ApiResponse<UserDto>.Ok(updated!.ToDto(), "User berhasil diperbarui.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var deleted = await _users.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return ApiResponse<bool>.Fail("User tidak ditemukan.");
            }

            await _users.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.Ok(true, "User berhasil dihapus.");
        }
    }
}
