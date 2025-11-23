using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Fənn yaratma/redaktə üçün ViewModel
/// </summary>
public class SubjectViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Fənn adı daxil edilməlidir")]
    [StringLength(200, ErrorMessage = "Fənn adı maksimum 200 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Kod maksimum 50 simvol ola bilər")]
    public string? Code { get; set; }

    [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Fənn detayları üçün ViewModel
/// </summary>
public class SubjectDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }

    // Statistika
    public int TeacherCount { get; set; }
    public int CompanyCount { get; set; }

    // Junction table məlumatları
    public List<TeacherListViewModel> Teachers { get; set; } = new();
    public List<CompanyListViewModel> Companies { get; set; } = new();
}

/// <summary>
/// Fənn siyahısı üçün ViewModel
/// </summary>
public class SubjectListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TeacherCount { get; set; }
    public int CompanyCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}