using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şirkət-İstifadəçi əlaqəsi (Manager, Admin)
/// </summary>
public class CompanyUser : AuditableEntity
{
    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    /// <summary>İstifadəçi ID</summary>
    public Guid UserId { get; set; }

    /// <summary>Manager statusu</summary>
    public bool IsManager { get; set; } = false;

    #region Navigation Properties
    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>İstifadəçi</summary>
    public virtual User User { get; set; } = null!;
    #endregion
}
