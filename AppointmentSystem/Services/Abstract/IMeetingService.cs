using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Görüş (Meeting) idarəetmə servisi
/// </summary>
public interface IMeetingService
{
    // Görüş yaratma
    Task<Meeting> CreateMeetingAsync(Guid teacherId, Guid parentId, Guid studentId, DateOnly date, TimeSpan startTime, string? note);
    Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(Guid parentId, CreateMeetingViewModel model);

    // Görüş idarəetməsi
    Task<bool> ApproveMeetingAsync(Guid meetingId, string? teacherResponse);
    Task<bool> ApproveMeetingAsync(Guid meetingId, Guid approvedById, string? teacherNotes);
    Task<bool> DeclineMeetingAsync(Guid meetingId, string declineReason, string? teacherResponse);
    Task<bool> DeclineMeetingAsync(Guid meetingId, Guid declinedById, string declineReason, string? teacherNotes);
    Task<bool> CancelMeetingAsync(Guid meetingId);
    Task<bool> CancelMeetingAsync(Guid meetingId, string? cancellationReason);

    // Sorğular
    Task<Meeting?> GetMeetingDetailsAsync(Guid meetingId);
    Task<List<TimeSpan>> GetAvailableTimeSlotsAsync(Guid teacherId, DateOnly date);
    Task<List<Meeting>> GetMeetingsByParentAsync(Guid parentId, MeetingStatus? status = null);
    Task<List<Meeting>> GetMeetingsByTeacherAsync(Guid teacherId, DateOnly? date = null, MeetingStatus? status = null);
    Task<List<TeacherMeetingViewModel>> GetTeacherMeetingsAsync(Guid teacherId, DateOnly? date = null);
    Task<List<ParentMeetingViewModel>> GetParentMeetingsAsync(Guid parentId, Guid companyId);
}