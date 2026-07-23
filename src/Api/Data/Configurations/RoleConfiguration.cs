using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Permissions).HasColumnType("text[]");
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new Role 
            { 
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                Name = "SystemAdmin", 
                Description = "Full system access", 
                Permissions = new List<string> { "*" }, 
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role 
            { 
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                Name = "Manager", 
                Description = "Manage clients and portfolios", 
                Permissions = new List<string> { "clients:read", "clients:write", "portfolios:read", "portfolios:write", "users:read", "audit:read" }, 
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role 
            { 
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), 
                Name = "Agent", 
                Description = "View clients and portfolios", 
                Permissions = new List<string> { "clients:read", "portfolios:read" }, 
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role 
            { 
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), 
                Name = "Viewer", 
                Description = "Read-only access", 
                Permissions = new List<string> { "clients:read", "portfolios:read" }, 
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
