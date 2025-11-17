using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Sinif-Müəllim əlaqəsi (M:M)
/// </summary>
public class ClassTeacher : AuditableEntity
{
    /// <summary>Sinif ID</summary>
    public Guid ClassId { get; set; }

    /// <summary>Müəllim ID</summary>
    public Guid TeacherId { get; set; }

    /// <summary>Sinif rəhbəri</summary>
    public bool IsClassLeader { get; set; } = false;

    #region Navigation Properties
    /// <summary>Sinif</summary>
    public virtual SchoolClass Class { get; set; } = null!;

    /// <summary>Müəllim</summary>
    public virtual Teacher Teacher { get; set; } = null!;
    #endregion
}
