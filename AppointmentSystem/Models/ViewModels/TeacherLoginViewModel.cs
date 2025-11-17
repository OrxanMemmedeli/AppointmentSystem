using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Müəllim login ViewModel
/// </summary>
public class TeacherLoginViewModel
{
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    [Required(ErrorMessage = "Email tələb olunur")]
    [EmailAddress(ErrorMessage = "Email format düzgün deyil")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə tələb olunur")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
