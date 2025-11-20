using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Role yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class RoleViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Rolun adı</summary>
    [Required(ErrorMessage = "Rol adı tələb olunur")]
    [StringLength(100, ErrorMessage = "Rol adı maksimum 100 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Rol kodu (unikal sistem identifikatoru)</summary>
    [StringLength(50, ErrorMessage = "Kod maksimum 50 simvol ola bilər")]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Kod yalnız böyük hərflər və alt xətt ola bilər")]
    public string? Code { get; set; }

    /// <summary>Rolun təsviri</summary>
    [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    /// <summary>Rol prioriteti (böyük rəqəm = yüksək prioritet)</summary>
    [Range(0, 100, ErrorMessage = "Prioritet 0-100 arasında olmalıdır")]
    public int Priority { get; set; }

    /// <summary>Sistem rolu işarəsi (silinə bilməz)</summary>
    public bool IsSystemRole { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}
