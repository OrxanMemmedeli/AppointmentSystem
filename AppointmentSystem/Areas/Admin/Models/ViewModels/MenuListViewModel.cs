using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Menu siyahısı üçün ViewModel
/// </summary>
public class MenuListViewModel
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int Level { get; set; }
    public string? IconSVG { get; set; }
    public string? Url { get; set; }
    public bool IsVisible { get; set; }
    public MenuType Type { get; set; }
    public bool IsActive { get; set; }
    public int ChildCount { get; set; }
    public string? ParentName { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string? AreaName { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
}
