using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.Entities;

// 3. Permission entity (əvvəlki OrganizationAdress)
public class Permission : AuditableEntity
{
    public string Name { get; set; } // Human-readable ad
    public string? Code { get; set; } // Unikal sistem kodu, məs: "USER_CREATE"
    public string? Description { get; set; }
    public string? ResourcePath { get; set; } // URL path: /controller/action
    public string? AreaName { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string HttpMethod { get; set; } = "GET"; // GET, POST, PUT, DELETE
    public PermissionType Type { get; set; } = PermissionType.Action; // Action, Menu, Feature
    public bool RequiresAuthentication { get; set; } = true;

    #region Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new HashSet<UserPermission>();
    #endregion
}
