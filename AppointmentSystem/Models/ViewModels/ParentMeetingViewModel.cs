using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.ViewModels;

public class ParentMeetingViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public MeetingStatus Status { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string? ParentNote { get; set; }
    public string? TeacherResponse { get; set; }
    public string? DeclineReason { get; set; }
}
