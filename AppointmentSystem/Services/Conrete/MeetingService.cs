using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Conrete;

/// <summary>
/// MeetingService implementation
/// </summary>
public class MeetingService : IMeetingService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MeetingService(AppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    #region Create Meeting

    public async Task<Meeting> CreateMeetingAsync(Guid teacherId, Guid parentId, Guid studentId, DateTime date, TimeSpan startTime, string? note)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
            throw new Exception("Müəllim tapılmadı");

        var duration = teacher.Company.DefaultMeetingDuration;
        var endTime = startTime.Add(TimeSpan.FromMinutes(duration));

        var conflictExists = await _context.Meetings
            .AnyAsync(m => m.TeacherId == teacherId &&
                          m.MeetingDate == date &&
                          m.StartTime < endTime &&
                          m.EndTime > startTime &&
                          m.Status != MeetingStatus.Cancelled &&
                          m.Status != MeetingStatus.Declined);

        if (conflictExists)
            throw new Exception("Bu vaxt artıq məşğuldur");

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            CompanyId = teacher.CompanyId,
            TeacherId = teacherId,
            ParentId = parentId,
            StudentId = studentId,
            MeetingDate = date,
            StartTime = startTime,
            EndTime = endTime,
            Status = MeetingStatus.Pending,
            ParentNotes = note,
            CreatedDate = DateTime.Now,
            CreatedById = _currentUserService.UserId,
            IsActive = true,
            IsDeleted = false
        };

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        return meeting;
    }

    public async Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(Guid parentId, CreateMeetingViewModel model)
    {
        try
        {
            var meeting = await CreateMeetingAsync(
                model.TeacherId,
                parentId,
                model.StudentId,
                model.MeetingDate,
                model.StartTime,
                model.ParentNotes
            );

            return (true, null, meeting.Id);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    #endregion

    #region Approve/Decline/Cancel

    public async Task<bool> ApproveMeetingAsync(Guid meetingId, string? teacherResponse)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Approved;
        meeting.TeacherNotes = teacherResponse;
        meeting.ApprovedDate = DateTime.Now;
        meeting.ApprovedById = _currentUserService.UserId;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = _currentUserService.UserId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveMeetingAsync(Guid meetingId, Guid approvedById, string? teacherNotes)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Approved;
        meeting.TeacherNotes = teacherNotes;
        meeting.ApprovedDate = DateTime.Now;
        meeting.ApprovedById = approvedById;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = approvedById;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeclineMeetingAsync(Guid meetingId, string declineReason, string? teacherResponse)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Declined;
        meeting.DeclineReason = declineReason;
        meeting.TeacherNotes = teacherResponse;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = _currentUserService.UserId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeclineMeetingAsync(Guid meetingId, Guid declinedById, string declineReason, string? teacherNotes)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Declined;
        meeting.DeclineReason = declineReason;
        meeting.TeacherNotes = teacherNotes;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = declinedById;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelMeetingAsync(Guid meetingId)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Cancelled;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = _currentUserService.UserId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelMeetingAsync(Guid meetingId, string? cancellationReason)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Cancelled;
        meeting.CancellationReason = cancellationReason;
        meeting.ModifiedDate = DateTime.Now;
        meeting.ModifiedById = _currentUserService.UserId;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Get Methods

    public async Task<Meeting?> GetMeetingDetailsAsync(Guid meetingId)
    {
        return await _context.Meetings
            .Include(m => m.Teacher)
            .Include(m => m.Parent)
            .Include(m => m.Student)
            .Include(m => m.Company)
            .FirstOrDefaultAsync(m => m.Id == meetingId);
    }

    public async Task<List<TimeSpan>> GetAvailableTimeSlotsAsync(Guid teacherId, DateTime date)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
            return new List<TimeSpan>();

        var company = teacher.Company;
        var startTime = company.DefaultStartTime;
        var endTime = company.DefaultEndTime;
        var meetingDuration = company.DefaultMeetingDuration;
        var breakDuration = company.DefaultBreakDuration;

        var meetings = await _context.Meetings
            .Where(m => m.TeacherId == teacherId &&
                       m.MeetingDate == date &&
                       m.Status != MeetingStatus.Cancelled &&
                       m.Status != MeetingStatus.Declined)
            .OrderBy(m => m.StartTime)
            .ToListAsync();

        var availableSlots = new List<TimeSpan>();
        var currentTime = startTime;

        while (currentTime.Add(TimeSpan.FromMinutes(meetingDuration)) <= endTime)
        {
            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(meetingDuration));

            var isOccupied = meetings.Any(m =>
                m.StartTime < slotEndTime && m.EndTime > currentTime);

            if (!isOccupied)
            {
                availableSlots.Add(currentTime);
            }

            currentTime = currentTime.Add(TimeSpan.FromMinutes(meetingDuration + breakDuration));
        }

        return availableSlots;
    }

    public async Task<List<Meeting>> GetMeetingsByParentAsync(Guid parentId, MeetingStatus? status = null)
    {
        var query = _context.Meetings
            .Include(m => m.Teacher)
            .Include(m => m.Student)
            .Where(m => m.ParentId == parentId);

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        return await query
            .OrderByDescending(m => m.MeetingDate)
            .ThenByDescending(m => m.StartTime)
            .ToListAsync();
    }

    public async Task<List<Meeting>> GetMeetingsByTeacherAsync(Guid teacherId, DateTime? date = null, MeetingStatus? status = null)
    {
        var query = _context.Meetings
            .Include(m => m.Parent)
            .Include(m => m.Student)
            .Where(m => m.TeacherId == teacherId);

        if (date.HasValue)
        {
            query = query.Where(m => m.MeetingDate == date.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        return await query
            .OrderBy(m => m.MeetingDate)
            .ThenBy(m => m.StartTime)
            .ToListAsync();
    }

    public async Task<List<TeacherMeetingViewModel>> GetTeacherMeetingsAsync(Guid teacherId, DateTime? date = null)
    {
        var query = _context.Meetings
            .Include(m => m.Parent)
            .Include(m => m.Student)
                .ThenInclude(s => s.Class)
            .Where(m => m.TeacherId == teacherId);

        if (date.HasValue)
        {
            query = query.Where(m => m.MeetingDate == date.Value);
        }

        var meetings = await query
            .OrderBy(m => m.MeetingDate)
            .ThenBy(m => m.StartTime)
            .ToListAsync();

        return meetings.Select(m => new TeacherMeetingViewModel
        {
            Id = m.Id,
            MeetingDate = m.MeetingDate,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            Status = m.Status,
            StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
            ParentName = $"{m.Parent.FirstName} {m.Parent.LastName}",
            ClassName = m.Student.Class?.Name ?? "N/A",
            ParentNote = m.ParentNotes,
            TeacherNotes = m.TeacherNotes
        }).ToList();
    }

    public async Task<List<ParentMeetingViewModel>> GetParentMeetingsAsync(Guid parentId, Guid companyId)
    {
        var meetings = await _context.Meetings
            .Include(m => m.Teacher)
            .Include(m => m.Student)
            .Where(m => m.ParentId == parentId && m.CompanyId == companyId)
            .OrderByDescending(m => m.MeetingDate)
            .ThenByDescending(m => m.StartTime)
            .ToListAsync();

        return meetings.Select(m => new ParentMeetingViewModel
        {
            Id = m.Id,
            MeetingDate = m.MeetingDate,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            Status = m.Status,
            TeacherName = $"{m.Teacher.FirstName} {m.Teacher.LastName}",
            StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
            ParentNote = m.ParentNotes,
            TeacherResponse = m.TeacherNotes,
            DeclineReason = m.DeclineReason
        }).ToList();
    }

    #endregion
}