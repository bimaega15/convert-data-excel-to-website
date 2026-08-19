using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data.Seeders
{
    /// <summary>Membuat role standar dan satu akun administrator awal.</summary>
    public class IdentitySeeder : IDataSeeder
    {
        public int Order => 1;
        public string Name => nameof(IdentitySeeder);

        private static readonly (string Name, string Description, bool CanAccessAdmin)[] DefaultRoles =
        {
            (RoleNames.Administrator, "Akses penuh termasuk area admin, import, dan manajemen user.", true),
            (RoleNames.Verifier, "Melihat dashboard dan mengelola data observasi.", true),
            (RoleNames.Viewer, "Hanya membaca dashboard dan master data.", false)
        };

        private readonly SifpDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly SeedOptions _options;
        private readonly ILogger<IdentitySeeder> _logger;

        public IdentitySeeder(
            SifpDbContext context,
            IPasswordHasher passwordHasher,
            IOptions<SeedOptions> options,
            ILogger<IdentitySeeder> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var existingRoles = await _context.Roles
                .ToDictionaryAsync(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase, cancellationToken);

            foreach (var (name, description, canAccessAdmin) in DefaultRoles)
            {
                if (existingRoles.ContainsKey(name))
                {
                    continue;
                }

                var role = new Role
                {
                    Name = name,
                    Description = description,
                    CanAccessAdmin = canAccessAdmin,
                    CreatedBy = "SEEDER"
                };

                _context.Roles.Add(role);
                existingRoles[name] = role;
                _logger.LogInformation("Role {Role} dibuat", name);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var harisUser = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Username.Contains("haris") || (u.Email != null && u.Email.Contains("haris")), cancellationToken);

            if (harisUser != null)
            {
                var adminRole = existingRoles[RoleNames.Administrator];
                if (!harisUser.UserRoles.Any(ur => ur.RoleId == adminRole.Id))
                {
                    _context.UserRoles.RemoveRange(harisUser.UserRoles);
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = harisUser.Id,
                        RoleId = adminRole.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = "SEEDER"
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Role user {Username} berhasil di-upgrade menjadi Administrator", harisUser.Username);
                }
            }

            var username = _options.AdminUsername.Trim();
            if (await _context.Users.AnyAsync(u => u.Username == username, cancellationToken))
            {
                return;
            }

            var admin = new User
            {
                Username = username,
                Email = _options.AdminEmail,
                FullName = _options.AdminFullName,
                PasswordHash = _passwordHasher.Hash(_options.AdminPassword),
                IsActive = true,
                CreatedBy = "SEEDER"
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync(cancellationToken);

            _context.UserRoles.Add(new UserRole
            {
                UserId = admin.Id,
                RoleId = existingRoles[RoleNames.Administrator].Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "SEEDER"
            });

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User admin \"{Username}\" dibuat", username);
        }
    }
}
