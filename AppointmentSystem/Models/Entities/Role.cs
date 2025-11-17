using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

// 2. Role entity - genişləndirilmiş
public class Role : AuditableEntity
{
    public string Name { get; set; }
    public string? Description { get; set; } // Əlavə edildi
    public string? Code { get; set; } // Sistem kodu üçün, məs: "ADMIN", "USER"
    public int Priority { get; set; } = 0; // Rol prioriteti
    public bool IsSystemRole { get; set; } = false; // Sistem rolu işarəsi

    #region Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new HashSet<RoleMenu>();
    #endregion
}
