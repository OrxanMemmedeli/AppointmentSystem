using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 5. UserRole - təkmilləşdirilmiş
public class UserRole : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset? AssignedDate { get; set; } // Rol verilmə tarixi
    public DateTimeOffset? ExpiryDate { get; set; } // Rolun bitmə tarixi
    public bool IsTemporary { get; set; } = false; // Müvəqqəti rol

    #region Navigation Properties
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
    #endregion
}
