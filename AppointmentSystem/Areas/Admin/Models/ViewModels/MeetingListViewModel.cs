using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Meeting list görüntüləmə ViewModel-i (Admin)
/// </summary>
public class MeetingListViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string TeacherName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }

    public string ParentName { get; set; } = string.Empty;
    public Guid ParentId { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }

    public string? Notes { get; set; } // ParentNotes

    public MeetingStatus Status { get; set; }
    public string StatusBadgeClass { get; set; } = "bg-secondary";
    public string StatusText { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsPast => MeetingDate < DateTime.Today;
    public bool IsToday => MeetingDate == DateTime.Today;
    public bool IsFuture => MeetingDate > DateTime.Today;
}

/// <summary>
/// Meeting Create/Edit ViewModel-i (Admin)
/// </summary>
public class MeetingViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Görüş tarixi seçilməlidir")]
    [Display(Name = "Görüş Tarixi")]
    public DateTime MeetingDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Başlama vaxtı seçilməlidir")]
    [Display(Name = "Başlama Vaxtı")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required(ErrorMessage = "Bitmə vaxtı seçilməlidir")]
    [Display(Name = "Bitmə Vaxtı")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(10, 0, 0);

    [Required(ErrorMessage = "Müəllim seçilməlidir")]
    [Display(Name = "Müəllim")]
    public Guid TeacherId { get; set; }

    [Required(ErrorMessage = "Valideyn seçilməlidir")]
    [Display(Name = "Valideyn")]
    public Guid ParentId { get; set; }

    [Display(Name = "Şagird")]
    public Guid? StudentId { get; set; }

    [Display(Name = "Şirkət")]
    public Guid? CompanyId { get; set; }

    [StringLength(1000, ErrorMessage = "Qeydlər maksimum 1000 simvol ola bilər")]
    [Display(Name = "Qeydlər")]
    public string? Notes { get; set; } // ParentNotes

    [Display(Name = "Status")]
    public MeetingStatus Status { get; set; } = MeetingStatus.Pending;
}

/// <summary>
/// Meeting Details ViewModel-i (Admin)
/// </summary>
public class MeetingDetailsViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    // Teacher info
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public string? TeacherPhone { get; set; }

    // Parent info
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }

    // Student info
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentClass { get; set; }

    // Meeting details
    public string? Notes { get; set; } // ParentNotes
    public string? TeacherNotes { get; set; }
    public string? DeclineReason { get; set; }
    public string? CancellationReason { get; set; }

    public MeetingStatus Status { get; set; }
    public string StatusBadgeClass { get; set; } = "bg-secondary";
    public string StatusText { get; set; } = string.Empty;

    // Audit info
    public DateTime CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }

    // Helper properties
    public bool CanApprove => Status == MeetingStatus.Pending;
    public bool CanComplete => Status == MeetingStatus.Approved && MeetingDate <= DateTime.Today;
    public bool CanCancel => Status != MeetingStatus.Completed && Status != MeetingStatus.Cancelled;
    public bool IsPast => MeetingDate < DateTime.Today;
}

/// <summary>
/// Meeting filter ViewModel-i (Admin)
/// </summary>
public class MeetingFilterViewModel
{
    [Display(Name = "Tarix (başlanğıc)")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Tarix (son)")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Müəllim")]
    public Guid? TeacherId { get; set; }

    [Display(Name = "Valideyn")]
    public Guid? ParentId { get; set; }

    [Display(Name = "Status")]
    public MeetingStatus? Status { get; set; }

    [Display(Name = "Axtarış")]
    [StringLength(100)]
    public string? SearchTerm { get; set; }

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Paginated result wrapper (Admin)
/// </summary>
public class PaginatedMeetingListViewModel
{
    public List<MeetingListViewModel> Meetings { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public MeetingFilterViewModel Filter { get; set; } = new();
}