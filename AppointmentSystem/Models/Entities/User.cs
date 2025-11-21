using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// İstifadəçi entity
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Ad</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Email</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>İstifadəçi adı</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Şifrə hash</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Telefon</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Şəkil</summary>
    public string? ImagePath { get; set; }

    /// <summary>Son giriş tarixi</summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>Email təsdiqlənib</summary>
    public bool IsEmailConfirmed { get; set; } = false;

    /// <summary>Hesab kilidlənib</summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>Uğursuz giriş cəhdləri</summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>Kilidlənmə bitmə tarixi</summary>
    public DateTime? LockoutEnd { get; set; }

    #region Navigation Properties
    /// <summary>İstifadəçi növü ID</summary>
    public Guid? UserTypeId { get; set; }

    /// <summary>İstifadəçi növü</summary>
    public virtual UserType? UserType { get; set; }

    /// <summary>İstifadəçi rolları</summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

    /// <summary>İstifadəçi icazələri</summary>
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new HashSet<UserPermission>();

    /// <summary>İstifadəçi menyuları</summary>
    public virtual ICollection<UserMenu> UserMenus { get; set; } = new HashSet<UserMenu>();

    /// <summary>Müəllim profili</summary>
    public virtual Teacher? Teacher { get; set; }

    /// <summary>Valideyn profili</summary>
    public virtual Parent? Parent { get; set; }

    /// <summary>Şirkət əlaqələri</summary>
    public virtual ICollection<CompanyUser> CompanyUsers { get; set; } = new HashSet<CompanyUser>();

    /// <summary>Müəssisə istifadəçiləri</summary>
    public virtual ICollection<InstitutionUser> InstitutionUsers { get; set; } = new HashSet<InstitutionUser>();

    #endregion
}
