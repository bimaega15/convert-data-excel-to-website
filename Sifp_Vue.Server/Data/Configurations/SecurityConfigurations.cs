using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);

            b.Property(x => x.Username).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();

            b.Property(x => x.Email).HasMaxLength(200);
            b.Property(x => x.FullName).HasMaxLength(200);
            b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            b.Property(x => x.MfaSecret).HasMaxLength(64);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);
        }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> b)
        {
            b.ToTable("Roles");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();

            b.Property(x => x.Description).HasMaxLength(300);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> b)
        {
            b.ToTable("UserRoles");
            b.HasKey(x => new { x.UserId, x.RoleId });

            b.Property(x => x.AssignedBy).HasMaxLength(100);

            b.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
