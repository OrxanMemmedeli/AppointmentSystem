using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Permission yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class PermissionViewModel
{
    public Guid? Id { get; set; }

    /// <summary>İcazənin adı</summary>
    [Required(ErrorMessage = "İcazə adı tələb olunur")]
    [StringLength(200, ErrorMessage = "İcazə adı maksimum 200 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Sistem kodu</summary>
    [StringLength(100, ErrorMessage = "Kod maksimum 100 simvol ola bilər")]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Kod yalnız böyük hərflər və alt xətt ola bilər")]
    public string? Code { get; set; }

    /// <summary>Təsvir</summary>
    [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    /// <summary>Resource path (URL)</summary>
    [StringLength(200)]
    public string? ResourcePath { get; set; }

    /// <summary>Area adı</summary>
    [StringLength(50)]
    public string? AreaName { get; set; }

    /// <summary>Controller adı</summary>
    [StringLength(100)]
    public string? ControllerName { get; set; }

    /// <summary>Action adı</summary>
    [StringLength(100)]
    public string? ActionName { get; set; }

    /// <summary>HTTP metodu</summary>
    [Required(ErrorMessage = "HTTP metodu tələb olunur")]
    public string HttpMethod { get; set; } = "GET";

    /// <summary>İcazə tipi</summary>
    [Required(ErrorMessage = "İcazə tipi tələb olunur")]
    public PermissionType Type { get; set; } = PermissionType.Action;

    /// <summary>Autentifikasiya tələb olunur</summary>
    public bool RequiresAuthentication { get; set; } = true;

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}
