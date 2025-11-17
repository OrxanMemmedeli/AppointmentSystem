using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Fənn
/// </summary>
public class Subject : AuditableEntity
{
    /// <summary>Fənn adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Fənn kodu</summary>
    public string? Code { get; set; }

    /// <summary>Açıqlama</summary>
    public string? Description { get; set; }

    #region Navigation Properties

    /// <summary>Bu fənni tədris edən müəllimlər</summary>
    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new HashSet<TeacherSubject>();

    /// <summary>Şirkət fənləri</summary>
    public virtual ICollection<CompanySubject> CompanySubjects { get; set; } = new HashSet<CompanySubject>();
    #endregion
}