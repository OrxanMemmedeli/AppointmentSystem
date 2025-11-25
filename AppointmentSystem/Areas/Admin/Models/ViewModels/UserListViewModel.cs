namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// İstifadəçi siyahısı üçün ViewModel
/// </summary>
public class UserListViewModel
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public bool IsLocked { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>Rolların siyahısı</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Display üçün rol string-i</summary>
    public string RolesDisplay => Roles.Any() ? string.Join(", ", Roles) : "-";
}