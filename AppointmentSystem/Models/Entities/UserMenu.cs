using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 9. UserMenu - təkmilləşdirilmiş
public class UserMenu : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid MenuId { get; set; }
    public bool HasAccess { get; set; } = true; // Giriş icazəsi
    public DateTimeOffset? ExpiryDate { get; set; } // İcazənin bitmə tarixi

    #region Navigation Properties
    public virtual User User { get; set; }
    public virtual Menu Menu { get; set; }
    #endregion
}
