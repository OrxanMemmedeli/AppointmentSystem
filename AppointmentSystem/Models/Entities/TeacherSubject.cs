using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Müəllim-Fənn əlaqəsi (M:M)
/// </summary>
public class TeacherSubject : AuditableEntity
{
    /// <summary>Müəllim ID</summary>
    public Guid TeacherId { get; set; }

    /// <summary>Fənn ID</summary>
    public Guid SubjectId { get; set; }

    #region Navigation Properties
    /// <summary>Müəllim</summary>
    public virtual Teacher Teacher { get; set; } = null!;

    /// <summary>Fənn</summary>
    public virtual Subject Subject { get; set; } = null!;
    #endregion
}
