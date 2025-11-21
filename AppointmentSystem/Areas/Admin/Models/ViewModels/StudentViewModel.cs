using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Şagird yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class StudentViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Ad</summary>
    [Required(ErrorMessage = "Ad tələb olunur")]
    [StringLength(100, ErrorMessage = "Ad maksimum 100 simvol ola bilər")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad</summary>
    [Required(ErrorMessage = "Soyad tələb olunur")]
    [StringLength(100, ErrorMessage = "Soyad maksimum 100 simvol ola bilər")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>FIN kod</summary>
    [Required(ErrorMessage = "FIN kod tələb olunur")]
    [StringLength(7, MinimumLength = 7, ErrorMessage = "FIN kod 7 simvoldan ibarət olmalıdır")]
    [RegularExpression(@"^[A-Z0-9]{7}$", ErrorMessage = "FIN kod yalnız böyük hərflər və rəqəmlərdən ibarət olmalıdır")]
    public string FinCode { get; set; } = string.Empty;

    /// <summary>Doğum tarixi</summary>
    [Required(ErrorMessage = "Doğum tarixi tələb olunur")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    /// <summary>Şəkil path</summary>
    [StringLength(500)]
    public string? ImagePath { get; set; }

    /// <summary>Şəkil file (upload üçün)</summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>Qeydlər</summary>
    [StringLength(2000)]
    public string? Notes { get; set; }

    /// <summary>Sinif ID</summary>
    [Required(ErrorMessage = "Sinif seçilməlidir")]
    public Guid ClassId { get; set; }

    /// <summary>Şirkət ID</summary>
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Tam ad (computed)</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Yaş (computed)</summary>
    public int Age => DateTime.Now.Year - DateOfBirth.Year -
        (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}

/// <summary>
/// Şagird siyahısı üçün ViewModel
/// </summary>
public class StudentListViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string FinCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.Now.Year - DateOfBirth.Year -
        (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public string? ImagePath { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ParentCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Şagird detayları üçün ViewModel
/// </summary>
public class StudentDetailsViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string FinCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.Now.Year - DateOfBirth.Year -
        (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public string? ImagePath { get; set; }
    public string? Notes { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int ClassLevel { get; set; }
    public string? ClassSection { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ParentCount { get; set; }
    public DateTime CreatedDate { get; set; }

    // Valideynlər
    public List<StudentParentInfo> Parents { get; set; } = new();
}

/// <summary>
/// Şagirdin valideyn məlumatı
/// </summary>
public class StudentParentInfo
{
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentFinCode { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
}
