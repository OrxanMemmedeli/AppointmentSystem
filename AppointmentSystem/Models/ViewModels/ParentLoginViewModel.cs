using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Valideyn login ViewModel
/// </summary>
public class ParentLoginViewModel
{
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    [Required(ErrorMessage = "Ad tələb olunur")]
    [StringLength(50, ErrorMessage = "Ad maksimum 50 simvol ola bilər")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad tələb olunur")]
    [StringLength(50, ErrorMessage = "Soyad maksimum 50 simvol ola bilər")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "FIN kod tələb olunur")]
    [StringLength(7, MinimumLength = 7, ErrorMessage = "FIN kod 7 simvol olmalıdır")]
    [RegularExpression(@"^[A-Z0-9]{7}$", ErrorMessage = "FIN kod format düzgün deyil")]
    public string FinCode { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
