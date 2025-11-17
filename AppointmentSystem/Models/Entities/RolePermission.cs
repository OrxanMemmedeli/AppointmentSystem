using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 6. RolePermission - OrganizationAdressRole əvəzinə
public class RolePermission : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool CanCreate { get; set; } = false; // CRUD permissions
    public bool CanRead { get; set; } = true;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;

    #region Navigation Properties
    public virtual Role Role { get; set; }
    public virtual Permission Permission { get; set; }
    #endregion
}
