using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Admin/Manager login ViewModel
/// </summary>
public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Email və ya İstifadəçi adı tələb olunur")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə tələb olunur")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
