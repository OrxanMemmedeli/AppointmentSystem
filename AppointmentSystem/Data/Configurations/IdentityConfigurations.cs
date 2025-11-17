using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

/// <summary>
/// User entity konfiqurasiyası
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.ImagePath)
            .HasMaxLength(500);

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // Relationships
        builder.HasOne(x => x.UserType)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.UserTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserPermissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserMenus)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Teacher)
            .WithOne(t => t.User)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Parent)
            .WithOne(p => p.User)
            .HasForeignKey<Parent>(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// Role entity konfiqurasiyası
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Relationships
        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RoleMenus)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// Permission entity konfiqurasiyası
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ResourcePath)
            .HasMaxLength(200);

        builder.Property(x => x.AreaName)
            .HasMaxLength(100);

        builder.Property(x => x.ControllerName)
            .HasMaxLength(100);

        builder.Property(x => x.ActionName)
            .HasMaxLength(100);

        builder.Property(x => x.HttpMethod)
            .HasMaxLength(10);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Relationships
        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserPermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// Menu entity konfiqurasiyası
/// </summary>
public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .HasMaxLength(100);

        builder.Property(x => x.IconSVG)
            .HasMaxLength(50);

        builder.Property(x => x.Url)
            .HasMaxLength(200);

        builder.Property(x => x.AreaName)
            .HasMaxLength(100);

        builder.Property(x => x.ControllerName)
            .HasMaxLength(100);

        builder.Property(x => x.ActionName)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Self-referencing relationship
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships
        builder.HasMany(x => x.UserMenus)
            .WithOne(x => x.Menu)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RoleMenus)
            .WithOne(x => x.Menu)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

/// <summary>
/// UserType entity konfiqurasiyası
/// </summary>
public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> builder)
    {
        builder.ToTable("UserTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Query filters
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
