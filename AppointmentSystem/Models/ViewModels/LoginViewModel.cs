using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email və ya İstifadəçi adı daxil edin")]
    [Display(Name = "Email və ya İstifadəçi adı")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə daxil edin")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifrə")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Məni yadda saxla")]
    public bool RememberMe { get; set; }
}
