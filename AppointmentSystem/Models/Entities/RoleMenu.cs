using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 10. RoleMenu - təkmilləşdirilmiş
public class RoleMenu : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }
    public bool HasAccess { get; set; } = true; // Giriş icazəsi
    public DateTime? ExpiryDate { get; set; }


    #region Navigation Properties
    public virtual Role Role { get; set; }
    public virtual Menu Menu { get; set; }
    #endregion
}
