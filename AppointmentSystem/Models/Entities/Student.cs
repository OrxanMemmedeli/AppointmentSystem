using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şagird
/// </summary>
public class Student : AuditableEntity
{
    /// <summary>Ad</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>FIN kod</summary>
    public string FinCode { get; set; } = string.Empty;

    /// <summary>Doğum tarixi</summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>Şəkil</summary>
    public string? ImagePath { get; set; }

    /// <summary>Qeydlər</summary>
    public string? Notes { get; set; }

    /// <summary>Sinif ID</summary>
    public Guid ClassId { get; set; }

    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    #region Navigation Properties
    /// <summary>Sinif</summary>
    public virtual SchoolClass Class { get; set; } = null!;

    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Valideynlər</summary>
    public virtual ICollection<StudentParent> StudentParents { get; set; } = new HashSet<StudentParent>();
    #endregion
}