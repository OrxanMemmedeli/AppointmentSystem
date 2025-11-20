using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Valideyn login ViewModel
/// </summary>
public class ParentLoginViewModel
{
    /// <summary>Şirkət identifikatoru</summary>
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Valideyn adının və soyadının baş hərfləri (məs: OM, AS və s.)
    /// </summary>
    [Required(ErrorMessage = "Ad və soyad baş hərfləri tələb olunur")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Baş hərflər minimum 2, maksimum 10 simvol olmalıdır")]
    [RegularExpression(@"^[A-ZƏÜÖĞÇŞİ]{2,10}$", ErrorMessage = "Yalnız böyük hərflər daxil edilə bilər (məs: OM, AS)")]
    public string Initials { get; set; } = string.Empty;

    /// <summary>FIN kod (avtomatik böyük hərfə çevrilir)</summary>
    [Required(ErrorMessage = "FIN kod tələb olunur")]
    [StringLength(7, MinimumLength = 7, ErrorMessage = "FIN kod 7 simvol olmalıdır")]
    [RegularExpression(@"^[A-Z0-9]{7}$", ErrorMessage = "FIN kod format düzgün deyil")]
    public string FinCode { get; set; } = string.Empty;

    /// <summary>Geri qayıdış URL-i</summary>
    public string? ReturnUrl { get; set; }
}
