using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Iyerarxik menyu strukturu üçün ViewModel
/// </summary>
public class MenuTreeViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? IconSVG { get; set; }
    public int Level { get; set; }
    public int OrderIndex { get; set; }
    public MenuType Type { get; set; }
    public bool IsVisible { get; set; }
    public bool IsActive { get; set; }
    public List<MenuTreeViewModel> Children { get; set; } = new();
}