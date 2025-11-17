using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.ViewModels;

public class TeacherMeetingViewModel
{
    public Guid Id { get; set; }
    public DateOnly MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public MeetingStatus Status { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? ParentNote { get; set; }
    public string? TeacherNotes { get; set; }
}
