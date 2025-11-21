namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Role siyahısı üçün ViewModel
/// </summary>
public class RoleListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public DateTime CreatedDate { get; set; }
}