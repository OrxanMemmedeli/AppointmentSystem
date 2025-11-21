using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Sinif yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class SchoolClassViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Sinif adı</summary>
    [Required(ErrorMessage = "Sinif adı tələb olunur")]
    [StringLength(50, ErrorMessage = "Sinif adı maksimum 50 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Sinif səviyyəsi (1-11)</summary>
    [Required(ErrorMessage = "Sinif səviyyəsi tələb olunur")]
    [Range(1, 11, ErrorMessage = "Sinif səviyyəsi 1-11 arasında olmalıdır")]
    public int Level { get; set; }

    /// <summary>Şöbə (A, B, C)</summary>
    [StringLength(10, ErrorMessage = "Şöbə maksimum 10 simvol ola bilər")]
    [RegularExpression(@"^[A-ZƏÜÖĞÇŞİ]+$", ErrorMessage = "Şöbə yalnız böyük hərflər ola bilər")]
    public string? Section { get; set; }

    /// <summary>Açıqlama</summary>
    [StringLength(500, ErrorMessage = "Açıqlama maksimum 500 simvol ola bilər")]
    public string? Description { get; set; }

    /// <summary>Şirkət ID</summary>
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Sinif siyahısı üçün ViewModel
/// </summary>
public class SchoolClassListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? Section { get; set; }
    public string? Description { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Sinif detayları üçün ViewModel
/// </summary>
public class SchoolClassDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? Section { get; set; }
    public string? Description { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public  DateTime CreatedDate { get; set; }

    // Əlavə statistika
    public List<StudentBasicInfo> Students { get; set; } = new();
    public List<TeacherBasicInfo> Teachers { get; set; } = new();
}

/// <summary>
/// Şagird əsas məlumatları
/// </summary>
public class StudentBasicInfo
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Müəllim əsas məlumatları
/// </summary>
public class TeacherBasicInfo
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}