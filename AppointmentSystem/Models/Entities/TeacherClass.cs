using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Müəllim-Sinif əlaqəsi
/// </summary>
public class TeacherClass : AuditableEntity
{
    /// <summary>Müəllim ID</summary>
    public Guid TeacherId { get; set; }

    /// <summary>Sinif ID</summary>
    public Guid ClassId { get; set; }

    /// <summary>Fənn ID</summary>
    public Guid? SubjectId { get; set; }

    /// <summary>Sinif rəhbəri</summary>
    public bool IsClassLeader { get; set; } = false;

    #region Navigation Properties
    /// <summary>Müəllim</summary>
    public virtual Teacher Teacher { get; set; } = null!;

    /// <summary>Sinif</summary>
    public virtual SchoolClass Class { get; set; } = null!;

    /// <summary>Fənn</summary>
    public virtual Subject? Subject { get; set; }
    #endregion
}