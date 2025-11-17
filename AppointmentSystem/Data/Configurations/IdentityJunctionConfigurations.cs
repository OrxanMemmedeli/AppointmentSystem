using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

/// <summary>
/// UserRole entity konfiqurasiyası
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.RoleId })
            .IsUnique();

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// UserPermission entity konfiqurasiyası
/// </summary>
public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.PermissionId })
            .IsUnique();

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// UserMenu entity konfiqurasiyası
/// </summary>
public class UserMenuConfiguration : IEntityTypeConfiguration<UserMenu>
{
    public void Configure(EntityTypeBuilder<UserMenu> builder)
    {
        builder.ToTable("UserMenus");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.MenuId })
            .IsUnique();

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// RolePermission entity konfiqurasiyası
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique();

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// RoleMenu entity konfiqurasiyası
/// </summary>
public class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("RoleMenus");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RoleId, x.MenuId })
            .IsUnique();

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
