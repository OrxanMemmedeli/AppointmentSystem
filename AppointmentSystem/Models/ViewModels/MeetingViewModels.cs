using AppointmentSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Görüş yaratma ViewModel
/// </summary>
public class CreateMeetingViewModel
{
    [Required(ErrorMessage = "Müəllim seçilməlidir")]
    public Guid TeacherId { get; set; }

    [Required(ErrorMessage = "Şagird seçilməlidir")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Tarix seçilməlidir")]
    [DataType(DataType.Date)]
    public DateTime MeetingDate { get; set; }

    [Required(ErrorMessage = "Vaxt seçilməlidir")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [StringLength(2000, ErrorMessage = "Qeyd maksimum 2000 simvol ola bilər")]
    public string? ParentNotes { get; set; }

    // Helper properties
    public string? TeacherName { get; set; }
    public string? StudentName { get; set; }
    public int Duration { get; set; } = 30; // Default 30 minutes
}

/// <summary>
/// Görüş detalları ViewModel
/// </summary>
public class MeetingDetailsViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public MeetingStatus Status { get; set; }
    public string StatusText => Status switch
    {
        MeetingStatus.Pending => "Gözləyir",
        MeetingStatus.Approved => "Təsdiqlənib",
        MeetingStatus.Declined => "İmtina edilib",
        MeetingStatus.Cancelled => "Ləğv edilib",
        MeetingStatus.Completed => "Tamamlanıb",
        _ => "Bilinmir"
    };

    // Teacher info
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherEmail { get; set; }
    public string? TeacherImagePath { get; set; }

    // Student info
    public string StudentName { get; set; } = string.Empty;
    public string? StudentClassName { get; set; }

    // Parent info
    public string ParentName { get; set; } = string.Empty;

    // Notes
    public string? ParentNotes { get; set; }
    public string? TeacherNotes { get; set; }
    public string? DeclineReason { get; set; }
    public string? CancellationReason { get; set; }

    // Timestamps
    public DateTime CreatedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

/// <summary>
/// Görüş siyahısı ViewModel
/// </summary>
//public class MeetingListViewModel
//{
//    public Guid Id { get; set; }
//    public DateTime MeetingDate { get; set; }
//    public TimeSpan StartTime { get; set; }
//    public TimeSpan EndTime { get; set; }
//    public MeetingStatus Status { get; set; }
//    public string StatusText { get; set; } = string.Empty;
//    public string TeacherName { get; set; } = string.Empty;
//    public string StudentName { get; set; } = string.Empty;
//    public string? ClassName { get; set; }
//    public bool CanCancel { get; set; }
//    public bool CanApprove { get; set; }
//}

/// <summary>
/// Müəllim təqvim ViewModel
/// </summary>
public class TeacherCalendarViewModel
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime SelectedDate { get; set; }
    public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    public List<TeacherMeetingViewModel> Meetings { get; set; } = new();
}

/// <summary>
/// Vaxt slot ViewModel
/// </summary>
public class TimeSlotViewModel
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBreak { get; set; }
    public string DisplayTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
}

/// <summary>
/// Görüş təsdiq/imtina ViewModel
/// </summary>
public class ApproveMeetingViewModel
{
    [Required]
    public Guid MeetingId { get; set; }

    [Required]
    public MeetingStatus NewStatus { get; set; }

    [StringLength(2000, ErrorMessage = "Qeyd maksimum 2000 simvol ola bilər")]
    public string? TeacherNotes { get; set; }

    [StringLength(1000, ErrorMessage = "İmtina səbəbi maksimum 1000 simvol ola bilər")]
    public string? DeclineReason { get; set; }
}

/// <summary>
/// Görüş ləğv etmə ViewModel
/// </summary>
public class CancelMeetingViewModel
{
    [Required]
    public Guid MeetingId { get; set; }

    [Required(ErrorMessage = "Ləğv səbəbi tələb olunur")]
    [StringLength(1000, ErrorMessage = "Ləğv səbəbi maksimum 1000 simvol ola bilər")]
    public string CancellationReason { get; set; } = string.Empty;
}

/// <summary>
/// Müsait vaxtlar sorğusu ViewModel
/// </summary>
public class AvailableTimeSlotsQueryViewModel
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
}
