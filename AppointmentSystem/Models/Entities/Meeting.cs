using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Görüş/Randevu
/// </summary>
public class Meeting : AuditableEntity
{
    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Müəllim ID</summary>
    public Guid TeacherId { get; set; }

    /// <summary>Valideyn ID</summary>
    public Guid ParentId { get; set; }

    /// <summary>Şagird ID</summary>
    public Guid StudentId { get; set; }

    /// <summary>Görüş tarixi</summary>
    public DateTime MeetingDate { get; set; }

    /// <summary>Başlanğıc vaxtı</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Bitmə vaxtı</summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>Status</summary>
    public MeetingStatus Status { get; set; } = MeetingStatus.Pending;

    /// <summary>Valideyn qeydləri</summary>
    public string? ParentNotes { get; set; }

    /// <summary>Müəllim qeydləri</summary>
    public string? TeacherNotes { get; set; }

    /// <summary>Təsdiq tarixi</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Təsdiq edən istifadəçi ID</summary>
    public Guid? ApprovedById { get; set; }

    /// <summary>Rədd səbəbi</summary>
    public string? DeclineReason { get; set; }

    /// <summary>Ləğv səbəbi</summary>
    public string? CancellationReason { get; set; }

    #region Navigation Properties
    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Müəllim</summary>
    public virtual Teacher Teacher { get; set; } = null!;

    /// <summary>Valideyn</summary>
    public virtual Parent Parent { get; set; } = null!;

    /// <summary>Şagird</summary>
    public virtual Student Student { get; set; } = null!;

    /// <summary>Təsdiq edən</summary>
    public virtual User? ApprovedBy { get; set; }
    #endregion
}