using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Menu yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class MenuViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Parent menyu ID-si (null = root level)</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Menyu adı</summary>
    [Required(ErrorMessage = "Menyu adı tələb olunur")]
    [StringLength(200, ErrorMessage = "Menyu adı maksimum 200 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Sistem kodu</summary>
    [StringLength(100, ErrorMessage = "Kod maksimum 100 simvol ola bilər")]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Kod yalnız böyük hərflər və alt xətt ola bilər")]
    public string? Code { get; set; }

    /// <summary>Təsvir</summary>
    [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    /// <summary>Sıralama indeksi</summary>
    [Range(0, 9999, ErrorMessage = "Sıralama 0-9999 arasında olmalıdır")]
    public int OrderIndex { get; set; }

    /// <summary>Səviyyə (avtomatik hesablanır)</summary>
    public int Level { get; set; }

    /// <summary>SVG ikonu</summary>
    [StringLength(50)]
    public string? IconSVG { get; set; }

    /// <summary>URL</summary>
    [StringLength(500)]
    public string? Url { get; set; }

    /// <summary>Area adı</summary>
    [StringLength(50)]
    public string? AreaName { get; set; }

    /// <summary>Controller adı</summary>
    [StringLength(100)]
    public string? ControllerName { get; set; }

    /// <summary>Action adı</summary>
    [StringLength(100)]
    public string? ActionName { get; set; }

    /// <summary>Görünəbilirlik</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Menyu tipi</summary>
    [Required(ErrorMessage = "Menyu tipi tələb olunur")]
    public MenuType Type { get; set; } = MenuType.Link;

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}
