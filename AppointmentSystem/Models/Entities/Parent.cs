using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Valideyn
/// </summary>
public class Parent : AuditableEntity
{
    /// <summary>Ad</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>FIN kod</summary>
    public string FinCode { get; set; } = string.Empty;

    /// <summary>Email</summary>
    public string? Email { get; set; }

    /// <summary>Telefon nömrəsi</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Şəkil</summary>
    public string? ImagePath { get; set; }

    /// <summary>İstifadəçi ID (User entity ilə əlaqə)</summary>
    public Guid? UserId { get; set; }

    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    #region Navigation Properties
    /// <summary>İstifadəçi məlumatları</summary>
    public virtual User? User { get; set; }

    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Şagirdlər</summary>
    public virtual ICollection<StudentParent> StudentParents { get; set; } = new HashSet<StudentParent>();

    /// <summary>Görüşlər</summary>
    public virtual ICollection<Meeting> Meetings { get; set; } = new HashSet<Meeting>();
    #endregion
}