using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Menyu ağacı görüntülənməsi üçün ViewModel
/// Hierarxik struktur - parent-child münasibəti ilə
/// </summary>
public class MenuTreeViewModel
{
    /// <summary>Menyu ID</summary>
    public Guid Id { get; set; }

    /// <summary>Parent menyu ID (null = root)</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Menyu adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Menyu kodu</summary>
    public string? Code { get; set; }

    /// <summary>İkon (Bootstrap Icons)</summary>
    public string? IconSVG { get; set; }

    /// <summary>URL (əgər varsa)</summary>
    public string? Url { get; set; }

    /// <summary>Area adı (MVC)</summary>
    public string? AreaName { get; set; }

    /// <summary>Controller adı (MVC)</summary>
    public string? ControllerName { get; set; }

    /// <summary>Action adı (MVC)</summary>
    public string? ActionName { get; set; }

    /// <summary>Səviyyə (0 = root, 1 = 1-ci səviyyə child və s.)</summary>
    public int Level { get; set; }

    /// <summary>Sıralama indeksi</summary>
    public int OrderIndex { get; set; }

    /// <summary>Menyu tipi</summary>
    public MenuType Type { get; set; }

    /// <summary>Görünən mi?</summary>
    public bool IsVisible { get; set; }

    /// <summary>Aktiv mi?</summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Child menyular (recursive)
    /// </summary>
    public List<MenuTreeViewModel> Children { get; set; } = new();

    /// <summary>
    /// Child sayı (UI-da göstərmək üçün)
    /// </summary>
    public int ChildCount => Children?.Count ?? 0;

    /// <summary>
    /// Bu menyunun child-ı var mı?
    /// </summary>
    public bool HasChildren => ChildCount > 0;
}