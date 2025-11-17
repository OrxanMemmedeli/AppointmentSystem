using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSystem.Models.Entities;

// 7. Menu - təkmilləşdirilmiş
public class Menu : AuditableEntity
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; } // Unikal sistem kodu
    public int OrderIndex { get; set; } = 0; // Sıralama üçün
    public int Level { get; set; } = 0; // Layer → Level (daha aydın)
    public string? IconSVG { get; set; }
    public string? Url { get; set; }
    public string? AreaName { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string? Description { get; set; }
    public bool IsVisible { get; set; } = true; // Görünəbilirlik
    public MenuType Type { get; set; } = MenuType.Link; // Link, Group, Separator

    #region Navigation Properties
    [ForeignKey("ParentId")]
    public virtual Menu? Parent { get; set; }
    public virtual ICollection<Menu> Children { get; set; } = new HashSet<Menu>();
    public virtual ICollection<UserMenu> UserMenus { get; set; } = new HashSet<UserMenu>();
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new HashSet<RoleMenu>();
    #endregion
}
