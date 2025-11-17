using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şirkət-Fənn əlaqəsi (M:M)
/// </summary>
public class CompanySubject : AuditableEntity
{
    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Fənn ID</summary>
    public Guid SubjectId { get; set; }

    #region Navigation Properties
    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Fənn</summary>
    public virtual Subject Subject { get; set; } = null!;
    #endregion
}
