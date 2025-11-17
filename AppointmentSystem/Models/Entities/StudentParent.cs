using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şagird-Valideyn əlaqəsi (M:M)
/// </summary>
public class StudentParent : AuditableEntity
{
    /// <summary>Şagird ID</summary>
    public Guid StudentId { get; set; }

    /// <summary>Valideyn ID</summary>
    public Guid ParentId { get; set; }

    /// <summary>Valideyn növü ID</summary>
    public Guid ParentTypeId { get; set; }

    /// <summary>Qohumluq növü</summary>
    public ParentRelationType RelationType { get; set; }

    /// <summary>Əsas valideyn (primary contact)</summary>
    public bool IsPrimaryContact { get; set; } = false;

    #region Navigation Properties
    /// <summary>Şagird</summary>
    public virtual Student Student { get; set; } = null!;

    /// <summary>Valideyn</summary>
    public virtual Parent Parent { get; set; } = null!;

    /// <summary>Valideyn növü</summary>
    public virtual ParentType ParentType { get; set; } = null!;
    #endregion
}