using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;
/// <summary>
/// Şirkət yaratmaq və redaktə etmək üçün ViewModel
/// </summary>
public class CompanyViewModel
{
    public Guid? Id { get; set; }

    /// <summary>Şirkət adı</summary>
    [Required(ErrorMessage = "Şirkət adı tələb olunur")]
    [StringLength(200, ErrorMessage = "Ad maksimum 200 simvol ola bilər")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Şirkət kodu (unikal)</summary>
    [Required(ErrorMessage = "Kod tələb olunur")]
    [StringLength(50, ErrorMessage = "Kod maksimum 50 simvol ola bilər")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Kod yalnız böyük hərflər, rəqəmlər və alt xətt ola bilər")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Email</summary>
    [Required(ErrorMessage = "Email tələb olunur")]
    [EmailAddress(ErrorMessage = "Email formatı düzgün deyil")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Telefon</summary>
    [Phone(ErrorMessage = "Telefon formatı düzgün deyil")]
    [StringLength(20)]
    public string? Phone { get; set; }

    /// <summary>Telefon nömrəsi (əlavə)</summary>
    [Phone(ErrorMessage = "Telefon formatı düzgün deyil")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>Ünvan</summary>
    [StringLength(500)]
    public string? Address { get; set; }

    /// <summary>Web sayt</summary>
    [Url(ErrorMessage = "URL formatı düzgün deyil")]
    [StringLength(200)]
    public string? Website { get; set; }

    /// <summary>Logo path</summary>
    [StringLength(500)]
    public string? LogoPath { get; set; }

    /// <summary>Logo file (upload üçün)</summary>
    public IFormFile? LogoFile { get; set; }

    /// <summary>Background image path</summary>
    [StringLength(500)]
    public string? BackgroundImagePath { get; set; }

    /// <summary>Background image file (upload üçün)</summary>
    public IFormFile? BackgroundImageFile { get; set; }

    /// <summary>Təsvir</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Xəritə URL (Google Maps)</summary>
    [Url(ErrorMessage = "URL formatı düzgün deyil")]
    [StringLength(500)]
    public string? MapUrl { get; set; }

    /// <summary>Xəritə koordinatları</summary>
    [StringLength(100)]
    public string? MapCoordinates { get; set; }

    /// <summary>Default görüş müddəti (dəqiqə)</summary>
    [Range(5, 480, ErrorMessage = "Görüş müddəti 5-480 dəqiqə arasında olmalıdır")]
    public int DefaultMeetingDuration { get; set; } = 30;

    /// <summary>Default fasilə müddəti (dəqiqə)</summary>
    [Range(0, 120, ErrorMessage = "Fasilə müddəti 0-120 dəqiqə arasında olmalıdır")]
    public int DefaultBreakDuration { get; set; } = 10;

    /// <summary>Default iş başlanğıc saatı</summary>
    [DataType(DataType.Time)]
    public TimeSpan DefaultStartTime { get; set; } = new TimeSpan(9, 0, 0);

    /// <summary>Default iş bitmə saatı</summary>
    [DataType(DataType.Time)]
    public TimeSpan DefaultEndTime { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>İş günləri (JSON)</summary>
    public string? WorkingDays { get; set; }

    /// <summary>Aktiv status</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Şirkət siyahısı üçün ViewModel
/// </summary>
public class CompanyListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int ClassCount { get; set; }
    public int SubjectCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}