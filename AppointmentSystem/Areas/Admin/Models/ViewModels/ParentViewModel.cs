using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Valideyn yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class ParentViewModel
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

    /// <summary>Email</summary>
    [EmailAddress(ErrorMessage = "Email formatı düzgün deyil")]
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>Telefon</summary>
    [Phone(ErrorMessage = "Telefon formatı düzgün deyil")]
    [StringLength(20)]
    public string? Phone { get; set; }

    /// <summary>Əlavə telefon</summary>
    [Phone(ErrorMessage = "Telefon formatı düzgün deyil")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>Şəkil path</summary>
    [StringLength(500)]
    public string? ImagePath { get; set; }

    /// <summary>Şəkil file (upload üçün)</summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>İstifadəçi ID</summary>
    public Guid? UserId { get; set; }

    /// <summary>Şirkət ID</summary>
    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Tam ad (computed)</summary>
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Valideyn siyahısı üçün ViewModel
/// </summary>
public class ParentListViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string FinCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ImagePath { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ChildrenCount { get; set; }
    public bool HasUser { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Valideyn detayları üçün ViewModel
/// </summary>
public class ParentDetailsViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string FinCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImagePath { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsActive { get; set; }
    public int ChildrenCount { get; set; }
    public int MeetingCount { get; set; }
    public DateTime CreatedDate { get; set; }

    // Uşaqlar
    public List<ParentChildInfo> Children { get; set; } = new();

    // Son görüşlər
    public List<ParentMeetingInfo> RecentMeetings { get; set; } = new();
}

/// <summary>
/// Valideynin uşaq məlumatı
/// </summary>
public class ParentChildInfo
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Valideyning görüş məlumatı
/// </summary>
public class ParentMeetingInfo
{
    public Guid MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }
    public string? TeacherName { get; set; }
    public string Status { get; set; } = string.Empty;
}
