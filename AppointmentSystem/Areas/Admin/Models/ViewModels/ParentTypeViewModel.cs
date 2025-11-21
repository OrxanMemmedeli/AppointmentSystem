using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Valideyn növü yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class ParentTypeViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Adı</summary>
    [Required(ErrorMessage = "Ad tələb olunur")]
    [StringLength(100, ErrorMessage = "Ad maksimum 100 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Açıqlama</summary>
    [StringLength(500, ErrorMessage = "Açıqlama maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    /// <summary>Növ (enum)</summary>
    [Required(ErrorMessage = "Növ seçilməlidir")]
    public ParentRelationType Type { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Valideyn növü siyahısı üçün ViewModel
/// </summary>
public class ParentTypeListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ParentRelationType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedDate { get; set; }
}