using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şirkət/Müəssisə
/// </summary>
public class Company : AuditableEntity
{
    /// <summary>Şirkət adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Şirkət kodu (unikal)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Ünvan</summary>
    public string? Address { get; set; }

    /// <summary>Telefon</summary>
    public string? Phone { get; set; }

    /// <summary>Telefon nömrəsi</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Email</summary>
    public string? Email { get; set; }

    /// <summary>Logo path</summary>
    public string? LogoPath { get; set; }

    /// <summary>Background image path</summary>
    public string? BackgroundImagePath { get; set; }

    /// <summary>Xəritə (Google Maps URL)</summary>
    public string? MapUrl { get; set; }

    /// <summary>Xəritə koordinatları</summary>
    public string? MapCoordinates { get; set; }

    /// <summary>Website</summary>
    public string? Website { get; set; }

    /// <summary>Əlavə məlumat</summary>
    public string? Description { get; set; }

    /// <summary>Default görüş müddəti (dəqiqələrlə)</summary>
    public int DefaultMeetingDuration { get; set; } = 30;

    /// <summary>Default görüşlər arası fasilə (dəqiqələrlə)</summary>
    public int DefaultBreakDuration { get; set; } = 10;

    /// <summary>Default iş başlanğıc saatı</summary>
    public TimeSpan DefaultStartTime { get; set; } = new TimeSpan(9, 0, 0);

    /// <summary>Default iş bitmə saatı</summary>
    public TimeSpan DefaultEndTime { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>İş günləri (JSON)</summary>
    public string? WorkingDays { get; set; }

    #region Navigation Properties
    /// <summary>Şirkətdəki siniflər</summary>
    public virtual ICollection<SchoolClass> Classes { get; set; } = new HashSet<SchoolClass>();

    /// <summary>Şirkətdəki müəllimlər</summary>
    public virtual ICollection<Teacher> Teachers { get; set; } = new HashSet<Teacher>();

    /// <summary>Şirkətdəki şagirdlər</summary>
    public virtual ICollection<Student> Students { get; set; } = new HashSet<Student>();

    /// <summary>Şirkətdəki fənlər</summary>
    public virtual ICollection<Subject> Subjects { get; set; } = new HashSet<Subject>();

    /// <summary>Şirkət parametrləri</summary>
    public virtual ICollection<CompanySetting> CompanySettings { get; set; } = new HashSet<CompanySetting>();

    /// <summary>Şirkət istifadəçiləri (Manager, Admin)</summary>
    public virtual ICollection<CompanyUser> CompanyUsers { get; set; } = new HashSet<CompanyUser>();

    /// <summary>Şirkət görüşləri</summary>
    public virtual ICollection<Meeting> Meetings { get; set; } = new HashSet<Meeting>();

    /// <summary>Müəssisə istifadəçiləri</summary>
    public virtual ICollection<InstitutionUser> InstitutionUsers { get; set; } = new HashSet<InstitutionUser>();
    public virtual ICollection<CompanySubject> CompanySubjects { get; set; } = new HashSet<CompanySubject>();
    #endregion
}