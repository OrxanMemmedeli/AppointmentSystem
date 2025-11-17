using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 11. UserPermission - birbaşa user permission üçün
public class UserPermission : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; } = true; // İcazə verilmiş/qadağan
    public DateTimeOffset? ExpiryDate { get; set; } // İcazənin bitmə tarixi
    public string? Reason { get; set; } // Xüsusi icazə səbəbi

    #region Navigation Properties
    public virtual User User { get; set; }
    public virtual Permission Permission { get; set; }
    #endregion
}
