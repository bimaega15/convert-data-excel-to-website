using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        IQueryable<User> Filter(QueryParameters query);

        /// <summary>Dipakai saat login: memuat role sekaligus agar claim bisa langsung disusun.</summary>
        Task<User?> GetByUsernameWithRolesAsync(string username, CancellationToken cancellationToken = default);

        Task<User?> GetWithRolesAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, int? exceptId = null, CancellationToken cancellationToken = default);
        Task ReplaceRolesAsync(int userId, IEnumerable<int> roleIds, string assignedBy, CancellationToken cancellationToken = default);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(SifpDbContext context) : base(context)
        {
        }

        public IQueryable<User> Filter(QueryParameters query)
        {
            var search = query.Search?.Trim();

            return Query()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    x.Username.Contains(search!) ||
                    (x.FullName != null && x.FullName.Contains(search!)) ||
                    (x.Email != null && x.Email.Contains(search!)))
                .OrderBy(x => x.Username);
        }

        public Task<User?> GetByUsernameWithRolesAsync(string username, CancellationToken cancellationToken = default)
            => Context.Users
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        public Task<User?> GetWithRolesAsync(int id, CancellationToken cancellationToken = default)
            => Query()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<bool> UsernameExistsAsync(string username, int? exceptId = null, CancellationToken cancellationToken = default)
            => Query().AnyAsync(x => x.Username == username && (exceptId == null || x.Id != exceptId), cancellationToken);

        /// <summary>
        /// Mengganti seluruh role user dengan daftar baru. Baris yang tidak berubah
        /// dibiarkan apa adanya supaya kolom AssignedAt/AssignedBy tidak ikut ter-reset.
        /// </summary>
        public async Task ReplaceRolesAsync(int userId, IEnumerable<int> roleIds, string assignedBy, CancellationToken cancellationToken = default)
        {
            var target = roleIds.Distinct().ToHashSet();

            var existing = await Context.UserRoles
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);

            var toRemove = existing.Where(x => !target.Contains(x.RoleId)).ToList();
            if (toRemove.Count > 0)
            {
                Context.UserRoles.RemoveRange(toRemove);
            }

            var existingIds = existing.Select(x => x.RoleId).ToHashSet();
            var toAdd = target
                .Where(roleId => !existingIds.Contains(roleId))
                .Select(roleId => new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = assignedBy
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                await Context.UserRoles.AddRangeAsync(toAdd, cancellationToken);
            }
        }
    }

    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RoleDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default);
    }

    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(SifpDbContext context) : base(context)
        {
        }

        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        public async Task<IReadOnlyList<RoleDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
            => await Query()
                .OrderBy(x => x.Name)
                .Select(x => new RoleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    CanAccessAdmin = x.CanAccessAdmin,
                    UserCount = x.UserRoles.Count
                })
                .ToListAsync(cancellationToken);
    }
}
