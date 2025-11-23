using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace AppointmentSystem.Data;

/// <summary>
/// Əsas verilənlər bazası konteksti
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    #region DbSets - Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserMenu> UserMenus => Set<UserMenu>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    #endregion

    #region DbSets - Domain
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<CompanySetting> CompanySettings => Set<CompanySetting>();
    public DbSet<ParentType> ParentTypes => Set<ParentType>();


    // Junction tables
    public DbSet<StudentParent> StudentParents => Set<StudentParent>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<CompanySubject> CompanySubjects => Set<CompanySubject>();
    public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();
    public DbSet<TeacherClass> TeacherClasses => Set<TeacherClass>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filters (Soft Delete)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GenerateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    /// <summary>
    /// Soft delete filter generator
    /// </summary>
    private static LambdaExpression GenerateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    /// <summary>
    /// SaveChanges override - audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.Now;
                    entry.Entity.IsActive = true;
                    entry.Entity.IsDeleted = false;
                    
                    if (entry.Entity is AuditableEntity auditableEntity)
                    {
                        // CreatedById will be set from service layer
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = DateTime.Now;
                    
                    if (entry.Entity is AuditableEntity modifiedAuditableEntity)
                    {
                        // ModifiedById will be set from service layer
                    }
                    break;

                case EntityState.Deleted:
                    // Soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.IsActive = false;
                    entry.Entity.ModifiedDate = DateTime.Now;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
