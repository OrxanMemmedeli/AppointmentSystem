using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Müəllim
/// </summary>
public class Teacher : AuditableEntity
{
    /// <summary>Ad</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Email</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Telefon nömrəsi</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Şəkil</summary>
    public string? ImagePath { get; set; }

    /// <summary>İxtisaslaşma</summary>
    public string? Specialization { get; set; }

    /// <summary>Bioqrafiya</summary>
    public string? Biography { get; set; }

    /// <summary>İstifadəçi ID (User entity ilə əlaqə)</summary>
    public Guid UserId { get; set; }

    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    #region Navigation Properties
    /// <summary>İstifadəçi məlumatları</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Müəllimin fənləri</summary>
    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new HashSet<TeacherSubject>();

    /// <summary>Müəllim-Sinif əlaqələri</summary>
    public virtual ICollection<TeacherClass> TeacherClasses { get; set; } = new HashSet<TeacherClass>();

    /// <summary>Müəllimin görüşləri</summary>
    public virtual ICollection<Meeting> Meetings { get; set; } = new HashSet<Meeting>();
    #endregion
}