using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Permission siyahısı üçün ViewModel
/// </summary>
public class PermissionListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? ResourcePath { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public PermissionType Type { get; set; }
    public bool RequiresAuthentication { get; set; }
    public bool IsActive { get; set; }
    public int RoleCount { get; set; }
    public int UserCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}