using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Görüş siyahısı üçün ViewModel (minimal - Teacher üçün)
/// </summary>
public class MeetingListViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public MeetingStatus Status { get; set; }


    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}