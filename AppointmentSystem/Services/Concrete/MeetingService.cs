using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AdminViewModels = AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Meeting servisi - Unified Implementation
/// </summary>
public class MeetingService : IMeetingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MeetingService> _logger;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUserService;

    private const string CACHE_KEY_TODAY_COUNT = "Meetings:TodayCount:{0}";
    private const string CACHE_KEY_PENDING_COUNT = "Meetings:PendingCount:{0}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    #region Compiled Queries

    private static readonly Func<AppDbContext, DateTime, Guid?, Task<int>> CountTodayMeetingsCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, DateTime today, Guid? companyId) =>
            ctx.Meetings.Count(m => !m.IsDeleted &&
                m.MeetingDate == today &&
                (!companyId.HasValue || m.CompanyId == companyId)));

    private static readonly Func<AppDbContext, Guid?, Task<int>> CountPendingMeetingsCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid? companyId) =>
            ctx.Meetings.Count(m => !m.IsDeleted &&
                m.Status == MeetingStatus.Pending &&
                (!companyId.HasValue || m.CompanyId == companyId)));

    private static readonly Func<AppDbContext, Guid, Task<Meeting?>> GetMeetingByIdCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid id) =>
            ctx.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Parent)
                .Include(m => m.Student)
                    .ThenInclude(s => s.Class)
                .Include(m => m.Company)
                .FirstOrDefault(m => m.Id == id && !m.IsDeleted));

    #endregion

    public MeetingService(
        AppDbContext context,
        ILogger<MeetingService> logger,
        IMemoryCache cache,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _currentUserService = currentUserService;
    }

    #region COMMON/SHARED METHODS

    public async Task<Meeting?> GetMeetingDetailsAsync(Guid meetingId)
    {
        try
        {
            return await GetMeetingByIdCompiled(_context, meetingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş detalları yüklənərkən xəta. ID: {MeetingId}", meetingId);
            return null;
        }
    }

    public async Task<List<TimeSpan>> GetAvailableTimeSlotsAsync(Guid teacherId, DateTime date)
    {
        try
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
                           m.Status != MeetingStatus.Declined &&
                           !m.IsDeleted)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müsait vaxt slotları yüklənərkən xəta. Teacher: {TeacherId}", teacherId);
            return new List<TimeSpan>();
        }
    }

    public async Task<bool> IsTeacherAvailableAsync(
        Guid teacherId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeMeetingId = null)
    {
        var conflictingMeeting = await _context.Meetings
            .AsNoTracking()
            .Where(m => !m.IsDeleted &&
                m.TeacherId == teacherId &&
                m.MeetingDate == date &&
                m.Status != MeetingStatus.Cancelled &&
                m.Status != MeetingStatus.Declined &&
                (excludeMeetingId == null || m.Id != excludeMeetingId) &&
                ((m.StartTime < endTime && m.EndTime > startTime)))
            .AnyAsync();

        return !conflictingMeeting;
    }

    public async Task<bool> IsParentAvailableAsync(
        Guid parentId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeMeetingId = null)
    {
        var conflictingMeeting = await _context.Meetings
            .AsNoTracking()
            .Where(m => !m.IsDeleted &&
                m.ParentId == parentId &&
                m.MeetingDate == date &&
                m.Status != MeetingStatus.Cancelled &&
                m.Status != MeetingStatus.Declined &&
                (excludeMeetingId == null || m.Id != excludeMeetingId) &&
                ((m.StartTime < endTime && m.EndTime > startTime)))
            .AnyAsync();

        return !conflictingMeeting;
    }

    #endregion

    #region ADMIN AREA METHODS

    public async Task<AdminViewModels.PaginatedMeetingListViewModel> GetMeetingsAsync(
        AdminViewModels.MeetingFilterViewModel filter,
        Guid? companyId = null)
    {
        try
        {
            var query = _context.Meetings
                .AsNoTracking()
                .Where(m => !m.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(m => m.CompanyId == companyId);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(m => m.MeetingDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(m => m.MeetingDate <= filter.EndDate.Value);
            }

            if (filter.TeacherId.HasValue)
            {
                query = query.Where(m => m.TeacherId == filter.TeacherId.Value);
            }

            if (filter.ParentId.HasValue)
            {
                query = query.Where(m => m.ParentId == filter.ParentId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(m => m.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(m =>
                    (m.ParentNotes != null && m.ParentNotes.ToLower().Contains(searchLower)) ||
                    (m.Teacher != null && (m.Teacher.FirstName + " " + m.Teacher.LastName).ToLower().Contains(searchLower)) ||
                    (m.Parent != null && (m.Parent.FirstName + " " + m.Parent.LastName).ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();

            var meetings = await query
                .OrderByDescending(m => m.MeetingDate)
                .ThenBy(m => m.StartTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(m => new AdminViewModels.MeetingListViewModel
                {
                    Id = m.Id,
                    MeetingDate = m.MeetingDate,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    TeacherId = m.TeacherId,
                    TeacherName = m.Teacher != null ? m.Teacher.FirstName + " " + m.Teacher.LastName : "",
                    ParentId = m.ParentId,
                    ParentName = m.Parent != null ? m.Parent.FirstName + " " + m.Parent.LastName : "",
                    StudentId = m.StudentId,
                    StudentName = m.Student != null ? m.Student.FirstName + " " + m.Student.LastName : "",
                    Notes = m.ParentNotes,
                    Status = m.Status,
                    StatusBadgeClass = GetStatusBadgeClass(m.Status),
                    StatusText = GetStatusText(m.Status),
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            return new AdminViewModels.PaginatedMeetingListViewModel
            {
                Meetings = meetings,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Filter = filter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşlər siyahısı yüklənərkən xəta");
            return new AdminViewModels.PaginatedMeetingListViewModel();
        }
    }

    public async Task<AdminViewModels.MeetingDetailsViewModel?> GetMeetingByIdAsync(Guid id)
    {
        try
        {
            var meeting = await GetMeetingByIdCompiled(_context, id);

            if (meeting == null)
                return null;

            return new AdminViewModels.MeetingDetailsViewModel
            {
                Id = meeting.Id,
                MeetingDate = meeting.MeetingDate,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,

                TeacherId = meeting.TeacherId,
                TeacherName = meeting.Teacher != null ? $"{meeting.Teacher.FirstName} {meeting.Teacher.LastName}" : "",
                TeacherEmail = meeting.Teacher?.Email ?? "",
                TeacherPhone = meeting.Teacher?.PhoneNumber,

                ParentId = meeting.ParentId,
                ParentName = meeting.Parent != null ? $"{meeting.Parent.FirstName} {meeting.Parent.LastName}" : "",
                ParentEmail = meeting.Parent?.Email ?? "",
                ParentPhone = meeting.Parent?.PhoneNumber,

                StudentId = meeting.StudentId,
                StudentName = meeting.Student != null ? $"{meeting.Student.FirstName} {meeting.Student.LastName}" : null,
                StudentClass = meeting.Student?.Class?.Name,

                Notes = meeting.ParentNotes,
                TeacherNotes = meeting.TeacherNotes,
                DeclineReason = meeting.DeclineReason,
                CancellationReason = meeting.CancellationReason,

                Status = meeting.Status,
                StatusBadgeClass = GetStatusBadgeClass(meeting.Status),
                StatusText = GetStatusText(meeting.Status),

                CreatedDate = meeting.CreatedDate,
                ModifiedDate = meeting.ModifiedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş məlumatları yüklənərkən xəta. ID: {MeetingId}", id);
            return null;
        }
    }

    public async Task<AdminViewModels.MeetingViewModel?> GetMeetingForEditAsync(Guid id)
    {
        try
        {
            var meeting = await _context.Meetings
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (meeting == null)
                return null;

            return new AdminViewModels.MeetingViewModel
            {
                Id = meeting.Id,
                MeetingDate = meeting.MeetingDate,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                TeacherId = meeting.TeacherId,
                ParentId = meeting.ParentId,
                StudentId = meeting.StudentId,
                CompanyId = meeting.CompanyId,
                Notes = meeting.ParentNotes,
                Status = meeting.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş edit məlumatları yüklənərkən xəta. ID: {MeetingId}", id);
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(
        AdminViewModels.MeetingViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (model.MeetingDate < DateTime.Today)
            {
                return (false, "Keçmiş tarixə görüş yaratmaq olmaz", null);
            }

            if (model.EndTime <= model.StartTime)
            {
                return (false, "Bitmə vaxtı başlama vaxtından sonra olmalıdır", null);
            }

            var teacherAvailable = await IsTeacherAvailableAsync(
                model.TeacherId, model.MeetingDate, model.StartTime, model.EndTime);

            if (!teacherAvailable)
            {
                return (false, "Müəllim bu vaxt müsait deyil", null);
            }

            var parentAvailable = await IsParentAvailableAsync(
                model.ParentId, model.MeetingDate, model.StartTime, model.EndTime);

            if (!parentAvailable)
            {
                return (false, "Valideyn bu vaxt müsait deyil", null);
            }

            var meetingId = Guid.NewGuid();
            var meeting = new Meeting
            {
                Id = meetingId,
                MeetingDate = model.MeetingDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                TeacherId = model.TeacherId,
                ParentId = model.ParentId,
                StudentId = model.StudentId.Value,
                CompanyId = model.CompanyId.Value,
                ParentNotes = model.Notes?.Trim(),
                Status = model.Status,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId,
                IsActive = true,
                IsDeleted = false
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            ClearMeetingCaches(model.CompanyId);

            _logger.LogInformation("Yeni görüş yaradıldı. ID: {MeetingId}", meetingId);

            return (true, null, meetingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yaradılarkən xəta");
            return (false, "Görüş yaradılarkən xəta baş verdi", null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateMeetingAsync(
        AdminViewModels.MeetingViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Meeting ID yoxdur");
            }

            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == model.Id.Value && !m.IsDeleted);

            if (meeting == null)
            {
                return (false, "Görüş tapılmadı");
            }

            if (model.EndTime <= model.StartTime)
            {
                return (false, "Bitmə vaxtı başlama vaxtından sonra olmalıdır");
            }

            if (meeting.MeetingDate != model.MeetingDate ||
                meeting.StartTime != model.StartTime ||
                meeting.EndTime != model.EndTime ||
                meeting.TeacherId != model.TeacherId)
            {
                var teacherAvailable = await IsTeacherAvailableAsync(
                    model.TeacherId, model.MeetingDate, model.StartTime, model.EndTime, model.Id);

                if (!teacherAvailable)
                {
                    return (false, "Müəllim bu vaxt müsait deyil");
                }
            }

            if (meeting.MeetingDate != model.MeetingDate ||
                meeting.StartTime != model.StartTime ||
                meeting.EndTime != model.EndTime ||
                meeting.ParentId != model.ParentId)
            {
                var parentAvailable = await IsParentAvailableAsync(
                    model.ParentId, model.MeetingDate, model.StartTime, model.EndTime, model.Id);

                if (!parentAvailable)
                {
                    return (false, "Valideyn bu vaxt müsait deyil");
                }
            }

            meeting.MeetingDate = model.MeetingDate;
            meeting.StartTime = model.StartTime;
            meeting.EndTime = model.EndTime;
            meeting.TeacherId = model.TeacherId;
            meeting.ParentId = model.ParentId;
            meeting.StudentId = model.StudentId.Value;
            meeting.ParentNotes = model.Notes?.Trim();
            meeting.Status = model.Status;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogInformation("Görüş yeniləndi. ID: {MeetingId}", meeting.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yenilənərkən xəta. ID: {MeetingId}", model.Id);
            return (false, "Görüş yenilənərkən xəta baş verdi");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteMeetingAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (meeting == null)
            {
                return (false, "Görüş tapılmadı");
            }

            meeting.IsDeleted = true;
            meeting.IsActive = false;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogWarning("Görüş silindi. ID: {MeetingId}", id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş silinərkən xəta. ID: {MeetingId}", id);
            return (false, "Görüş silinərkən xəta baş verdi");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CompleteMeetingAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (meeting == null)
            {
                return (false, "Görüş tapılmadı");
            }

            if (meeting.Status == MeetingStatus.Completed)
            {
                return (false, "Görüş artıq tamamlanıb");
            }

            if (meeting.Status == MeetingStatus.Cancelled)
            {
                return (false, "Ləğv edilmiş görüş tamamlana bilməz");
            }

            meeting.Status = MeetingStatus.Completed;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogInformation("Görüş tamamlandı. ID: {MeetingId}", id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş tamamlanarkən xəta. ID: {MeetingId}", id);
            return (false, "Görüş tamamlanarkən xəta baş verdi");
        }
    }

    public async Task<int> GetTodayMeetingsCountAsync(Guid? companyId = null)
    {
        var cacheKey = string.Format(CACHE_KEY_TODAY_COUNT, companyId?.ToString() ?? "global");

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);
            return await CountTodayMeetingsCompiled(_context, DateTime.Today, companyId);
        });

        return result;
    }

    public async Task<int> GetPendingMeetingsCountAsync(Guid? companyId = null)
    {
        var cacheKey = string.Format(CACHE_KEY_PENDING_COUNT, companyId?.ToString() ?? "global");

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);
            return await CountPendingMeetingsCompiled(_context, companyId);
        });

        return result;
    }

    #endregion

    #region TEACHER AREA METHODS

    public async Task<List<TeacherMeetingViewModel>> GetTeacherMeetingsAsync(
        Guid teacherId,
        DateTime? date = null)
    {
        try
        {
            var query = _context.Meetings
                .Include(m => m.Parent)
                .Include(m => m.Student)
                    .ThenInclude(s => s.Class)
                .Where(m => m.TeacherId == teacherId && !m.IsDeleted);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teacher görüşləri yüklənərkən xəta. Teacher: {TeacherId}", teacherId);
            return new List<TeacherMeetingViewModel>();
        }
    }

    public async Task<List<Meeting>> GetMeetingsByTeacherAsync(
        Guid teacherId,
        DateTime? date = null,
        MeetingStatus? status = null)
    {
        try
        {
            var query = _context.Meetings
                .Include(m => m.Parent)
                .Include(m => m.Student)
                .Where(m => m.TeacherId == teacherId && !m.IsDeleted);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teacher görüşləri (entity) yüklənərkən xəta. Teacher: {TeacherId}", teacherId);
            return new List<Meeting>();
        }
    }

    public async Task<bool> ApproveMeetingAsync(
        Guid meetingId,
        Guid approvedById,
        string? teacherNotes)
    {
        try
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

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogInformation("Görüş təsdiqləndi. ID: {MeetingId}", meetingId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş təsdiqlənilərkən xəta. ID: {MeetingId}", meetingId);
            return false;
        }
    }

    public async Task<bool> DeclineMeetingAsync(
        Guid meetingId,
        Guid declinedById,
        string declineReason,
        string? teacherNotes)
    {
        try
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null) return false;

            meeting.Status = MeetingStatus.Declined;
            meeting.DeclineReason = declineReason;
            meeting.TeacherNotes = teacherNotes;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedById = declinedById;

            await _context.SaveChangesAsync();

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogWarning("Görüşdən imtina edildi. ID: {MeetingId}, Səbəb: {Reason}", meetingId, declineReason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşdən imtina edilərkən xəta. ID: {MeetingId}", meetingId);
            return false;
        }
    }

    #endregion

    #region PARENT AREA METHODS

    public async Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(
        Guid parentId,
        CreateMeetingViewModel model)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.Company)
                .FirstOrDefaultAsync(t => t.Id == model.TeacherId);

            if (teacher == null)
                return (false, "Müəllim tapılmadı", null);

            var duration = teacher.Company.DefaultMeetingDuration;
            var endTime = model.StartTime.Add(TimeSpan.FromMinutes(duration));

            var conflictExists = await _context.Meetings
                .AnyAsync(m => m.TeacherId == model.TeacherId &&
                              m.MeetingDate == model.MeetingDate &&
                              m.StartTime < endTime &&
                              m.EndTime > model.StartTime &&
                              m.Status != MeetingStatus.Cancelled &&
                              m.Status != MeetingStatus.Declined &&
                              !m.IsDeleted);

            if (conflictExists)
                return (false, "Bu vaxt artıq məşğuldur", null);

            var meetingId = Guid.NewGuid();
            var meeting = new Meeting
            {
                Id = meetingId,
                CompanyId = teacher.CompanyId,
                TeacherId = model.TeacherId,
                ParentId = parentId,
                StudentId = model.StudentId,
                MeetingDate = model.MeetingDate,
                StartTime = model.StartTime,
                EndTime = endTime,
                Status = MeetingStatus.Pending,
                ParentNotes = model.ParentNotes,
                CreatedDate = DateTime.Now,
                CreatedById = _currentUserService.UserId ?? Guid.Empty,
                IsActive = true,
                IsDeleted = false
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            ClearMeetingCaches(teacher.CompanyId);

            _logger.LogInformation("Parent tərəfindən görüş yaradıldı. ID: {MeetingId}", meetingId);

            return (true, null, meetingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parent görüş yaradarkən xəta");
            return (false, ex.Message, null);
        }
    }

    public async Task<List<ParentMeetingViewModel>> GetParentMeetingsAsync(
        Guid parentId,
        Guid companyId)
    {
        try
        {
            var meetings = await _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Student)
                .Where(m => m.ParentId == parentId && m.CompanyId == companyId && !m.IsDeleted)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parent görüşləri yüklənərkən xəta. Parent: {ParentId}", parentId);
            return new List<ParentMeetingViewModel>();
        }
    }

    public async Task<List<Meeting>> GetMeetingsByParentAsync(
        Guid parentId,
        MeetingStatus? status = null)
    {
        try
        {
            var query = _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Student)
                .Where(m => m.ParentId == parentId && !m.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            return await query
                .OrderByDescending(m => m.MeetingDate)
                .ThenByDescending(m => m.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parent görüşləri (entity) yüklənərkən xəta. Parent: {ParentId}", parentId);
            return new List<Meeting>();
        }
    }

    public async Task<bool> CancelMeetingAsync(
        Guid meetingId,
        string? cancellationReason)
    {
        try
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null) return false;

            meeting.Status = MeetingStatus.Cancelled;
            meeting.CancellationReason = cancellationReason;
            meeting.ModifiedDate = DateTime.Now;
            meeting.ModifiedById = _currentUserService.UserId ?? Guid.Empty;

            await _context.SaveChangesAsync();

            ClearMeetingCaches(meeting.CompanyId);

            _logger.LogWarning("Görüş ləğv edildi. ID: {MeetingId}, Səbəb: {Reason}", meetingId, cancellationReason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş ləğv edilərkən xəta. ID: {MeetingId}", meetingId);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private string GetStatusBadgeClass(MeetingStatus status)
    {
        return status switch
        {
            MeetingStatus.Pending => "bg-warning",
            MeetingStatus.Approved => "bg-primary",
            MeetingStatus.Completed => "bg-success",
            MeetingStatus.Cancelled => "bg-danger",
            MeetingStatus.Declined => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    private string GetStatusText(MeetingStatus status)
    {
        return status switch
        {
            MeetingStatus.Pending => "Gözləyir",
            MeetingStatus.Approved => "Təsdiqləndi",
            MeetingStatus.Completed => "Tamamlandı",
            MeetingStatus.Cancelled => "Ləğv edildi",
            MeetingStatus.Declined => "İmtina edildi",
            _ => "Naməlum"
        };
    }

    private void ClearMeetingCaches(Guid? companyId)
    {
        _cache.Remove(string.Format(CACHE_KEY_TODAY_COUNT, companyId?.ToString() ?? "global"));
        _cache.Remove(string.Format(CACHE_KEY_PENDING_COUNT, companyId?.ToString() ?? "global"));

        _logger.LogDebug("Meeting cache-ləri təmizləndi. CompanyId: {CompanyId}", companyId);
    }

    #endregion
}