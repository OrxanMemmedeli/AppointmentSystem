namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Login response ViewModel
/// </summary>
public class LoginResultViewModel
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public string? UserRole { get; set; }
}
