using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Şagird-Valideyn əlaqəsi yaratmaq üçün ViewModel
/// </summary>
public class StudentParentViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Şagird ID</summary>
    [Required(ErrorMessage = "Şagird seçilməlidir")]
    public Guid StudentId { get; set; }

    /// <summary>Valideyn ID</summary>
    [Required(ErrorMessage = "Valideyn seçilməlidir")]
    public Guid ParentId { get; set; }

    /// <summary>Valideyn növü ID</summary>
    [Required(ErrorMessage = "Valideyn növü seçilməlidir")]
    public Guid ParentTypeId { get; set; }

    /// <summary>Qohumluq növü</summary>
    [Required(ErrorMessage = "Qohumluq növü seçilməlidir")]
    public ParentRelationType RelationType { get; set; }

    /// <summary>Əsas valideyn</summary>
    public bool IsPrimaryContact { get; set; } = false;

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Şagird-Valideyn əlaqəsi siyahısı üçün ViewModel
/// </summary>
public class StudentParentListViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentClassName { get; set; }
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentFinCode { get; set; } = string.Empty;
    public Guid ParentTypeId { get; set; }
    public string ParentTypeName { get; set; } = string.Empty;
    public ParentRelationType RelationType { get; set; }
    public string RelationTypeDisplay { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}