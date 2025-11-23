using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Müəllim yaratma/redaktə üçün ViewModel
/// </summary>
public class TeacherViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Ad daxil edilməlidir")]
    [StringLength(100, ErrorMessage = "Ad maksimum 100 simvol ola bilər")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad daxil edilməlidir")]
    [StringLength(100, ErrorMessage = "Soyad maksimum 100 simvol ola bilər")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email daxil edilməlidir")]
    [EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
    [StringLength(150, ErrorMessage = "Email maksimum 150 simvol ola bilər")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin")]
    [StringLength(20, ErrorMessage = "Telefon maksimum 20 simvol ola bilər")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "İxtisaslaşma maksimum 200 simvol ola bilər")]
    public string? Specialization { get; set; }

    [StringLength(1000, ErrorMessage = "Bioqrafiya maksimum 1000 simvol ola bilər")]
    public string? Biography { get; set; }

    [Required(ErrorMessage = "İstifadəçi seçilməlidir")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    public string? ImagePath { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Müəllim detayları üçün ViewModel
/// </summary>
public class TeacherDetailsViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ImagePath { get; set; }
    public string? Specialization { get; set; }
    public string? Biography { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }

    // Əlaqəli məlumatlar
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    // Statistika
    public int SubjectCount { get; set; }
    public int ClassCount { get; set; }
    public int MeetingCount { get; set; }

    // Junction table məlumatları
    public List<SubjectListViewModel> Subjects { get; set; } = new();
    public List<TeacherClassViewModel> Classes { get; set; } = new();
    public List<MeetingListViewModel> RecentMeetings { get; set; } = new();
}

/// <summary>
/// Müəllim-Sinif əlaqəsi üçün ViewModel
/// </summary>
public class TeacherClassViewModel
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public bool IsClassLeader { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}