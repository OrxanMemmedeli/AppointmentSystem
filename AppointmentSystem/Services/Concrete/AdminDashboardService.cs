using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Admin dashboard servisi - Production-ready implementasiya
/// Compiled Queries, Caching, Multi-tenant support
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly IMemoryCache _cache;

    // Cache keys - Admin-specific
    private const string CACHE_KEY_ADMIN_DASHBOARD = "AdminDashboard:Full:{0}";
    private const string CACHE_KEY_ADMIN_STATISTICS = "AdminDashboard:Statistics:{0}";
    private const string CACHE_KEY_ADMIN_TRENDS = "AdminDashboard:Trends:{0}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    #region Compiled Queries - PERFORMANS KRİTİK

    private static readonly Func<AppDbContext, Task<int>> CountActiveCompaniesCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx) =>
            ctx.Companies.Count(c => c.IsActive && !c.IsDeleted));

    private static readonly Func<AppDbContext, Guid?, Task<int>> CountActiveTeachersCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid? companyId) =>
            ctx.Teachers.Count(t => t.IsActive && !t.IsDeleted &&
                (!companyId.HasValue || t.CompanyId == companyId)));

    private static readonly Func<AppDbContext, Guid?, Task<int>> CountActiveStudentsCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid? companyId) =>
            ctx.Students.Count(s => s.IsActive && !s.IsDeleted &&
                (!companyId.HasValue || s.CompanyId == companyId)));

    private static readonly Func<AppDbContext, Guid?, Task<int>> CountActiveParentsCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid? companyId) =>
            ctx.Parents.Count(p => p.IsActive && !p.IsDeleted &&
                (!companyId.HasValue || p.CompanyId == companyId)));

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

    #endregion

    public AdminDashboardService(
        AppDbContext context,
        ILogger<AdminDashboardService> logger,
        IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Admin dashboard üçün tam məlumatları gətirir (cached)
    /// </summary>
    public async Task<DashboardViewModel> GetDashboardDataAsync(Guid? companyId = null)
    {
        var cacheKey = string.Format(CACHE_KEY_ADMIN_DASHBOARD, companyId?.ToString() ?? "global");

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);

            try
            {
                _logger.LogInformation("Admin Dashboard məlumatları yüklənir. CompanyId: {CompanyId}", companyId);

                // Parallel sorğularla performans artırımı
                var statisticsTask = GetStatisticsAsync(companyId);
                var trendsTask = GetTrendsAsync(companyId);
                var activitiesTask = GetRecentActivitiesAsync(companyId, 10);
                var meetingsTask = GetUpcomingMeetingsAsync(companyId, 7);
                var performersTask = GetTopPerformersAsync(companyId, 5);

                await Task.WhenAll(statisticsTask, trendsTask, activitiesTask, meetingsTask, performersTask);

                return new DashboardViewModel
                {
                    Statistics = await statisticsTask,
                    Trends = await trendsTask,
                    RecentActivities = await activitiesTask,
                    UpcomingMeetings = await meetingsTask,
                    TopPerformers = await performersTask
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Dashboard məlumatları yüklənərkən xəta. CompanyId: {CompanyId}", companyId);
                return new DashboardViewModel();
            }
        }) ?? new DashboardViewModel();
    }

    /// <summary>
    /// Statistika kartları (compiled + cached)
    /// </summary>
    public async Task<DashboardStatisticsViewModel> GetStatisticsAsync(Guid? companyId = null)
    {
        var cacheKey = string.Format(CACHE_KEY_ADMIN_STATISTICS, companyId?.ToString() ?? "global");

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);

            var today = DateTime.Today;
            var lastMonth = today.AddMonths(-1);
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);

            // Parallel compiled query execution
            var companiesTask = companyId.HasValue
                ? Task.FromResult(1)
                : CountActiveCompaniesCompiled(_context);

            var teachersTask = CountActiveTeachersCompiled(_context, companyId);
            var studentsTask = CountActiveStudentsCompiled(_context, companyId);
            var parentsTask = CountActiveParentsCompiled(_context, companyId);
            var todayMeetingsTask = CountTodayMeetingsCompiled(_context, today, companyId);
            var pendingMeetingsTask = CountPendingMeetingsCompiled(_context, companyId);

            // Bu ayın completed və cancelled görüşləri
            var completedThisMonthTask = _context.Meetings
                .Where(m => !m.IsDeleted &&
                    m.Status == MeetingStatus.Completed &&
                    m.MeetingDate >= thisMonthStart &&
                    (!companyId.HasValue || m.CompanyId == companyId))
                .CountAsync();

            var cancelledThisMonthTask = _context.Meetings
                .Where(m => !m.IsDeleted &&
                    m.Status == MeetingStatus.Cancelled &&
                    m.MeetingDate >= thisMonthStart &&
                    (!companyId.HasValue || m.CompanyId == companyId))
                .CountAsync();

            // Keçən ayın sayları (trend hesablamaq üçün)
            var teachersLastMonthTask = _context.Teachers
                .Where(t => t.IsActive && !t.IsDeleted &&
                    t.CreatedDate < lastMonth &&
                    (!companyId.HasValue || t.CompanyId == companyId))
                .CountAsync();

            var studentsLastMonthTask = _context.Students
                .Where(s => s.IsActive && !s.IsDeleted &&
                    s.CreatedDate < lastMonth &&
                    (!companyId.HasValue || s.CompanyId == companyId))
                .CountAsync();

            await Task.WhenAll(
                companiesTask, teachersTask, studentsTask, parentsTask,
                todayMeetingsTask, pendingMeetingsTask,
                completedThisMonthTask, cancelledThisMonthTask,
                teachersLastMonthTask, studentsLastMonthTask
            );

            var totalTeachers = await teachersTask;
            var totalStudents = await studentsTask;
            var teachersLastMonth = await teachersLastMonthTask;
            var studentsLastMonth = await studentsLastMonthTask;

            return new DashboardStatisticsViewModel
            {
                TotalCompanies = await companiesTask,
                CompaniesChangePercent = 0,

                TotalTeachers = totalTeachers,
                TeachersChangePercent = CalculateChangePercent(totalTeachers, teachersLastMonth),

                TotalStudents = totalStudents,
                StudentsChangePercent = CalculateChangePercent(totalStudents, studentsLastMonth),

                TotalParents = await parentsTask,
                ParentsChangePercent = 0,

                TodayMeetings = await todayMeetingsTask,
                TodayMeetingsChangePercent = 0,

                PendingMeetings = await pendingMeetingsTask,
                PendingMeetingsChangePercent = 0,

                CompletedMeetingsThisMonth = await completedThisMonthTask,
                CancelledMeetingsThisMonth = await cancelledThisMonthTask
            };
        }) ?? new DashboardStatisticsViewModel();
    }

    /// <summary>
    /// Aylıq trend məlumatları (son 6 ay)
    /// </summary>
    public async Task<DashboardTrendsViewModel> GetTrendsAsync(Guid? companyId = null)
    {
        var cacheKey = string.Format(CACHE_KEY_ADMIN_TRENDS, companyId?.ToString() ?? "global");

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            var today = DateTime.Today;
            var sixMonthsAgo = today.AddMonths(-6);

            // Son 6 ayın görüş statistikası
            var meetingStats = await _context.Meetings
                .Where(m => !m.IsDeleted &&
                    m.MeetingDate >= sixMonthsAgo &&
                    (!companyId.HasValue || m.CompanyId == companyId))
                .GroupBy(m => new { m.MeetingDate.Year, m.MeetingDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Completed = g.Count(m => m.Status == MeetingStatus.Completed),
                    Cancelled = g.Count(m => m.Status == MeetingStatus.Cancelled),
                    Pending = g.Count(m => m.Status == MeetingStatus.Pending)
                })
                .ToListAsync();

            // Azərbaycan dilində ay adları
            var azMonths = new[] { "Yanvar", "Fevral", "Mart", "Aprel", "May", "İyun",
                                    "İyul", "Avqust", "Sentyabr", "Oktyabr", "Noyabr", "Dekabr" };

            var meetingTrends = meetingStats.Select(m => new MonthlyMeetingTrend
            {
                Month = $"{azMonths[m.Month - 1]} {m.Year}",
                Completed = m.Completed,
                Cancelled = m.Cancelled,
                Pending = m.Pending
            }).ToList();

            return new DashboardTrendsViewModel
            {
                MeetingTrends = meetingTrends,
                UserGrowth = new List<MonthlyUserGrowth>()
            };
        }) ?? new DashboardTrendsViewModel();
    }

    /// <summary>
    /// Son 10 fəaliyyət
    /// </summary>
    public async Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(Guid? companyId = null, int count = 10)
    {
        var recentMeetings = await _context.Meetings
            .Where(m => !m.IsDeleted && (!companyId.HasValue || m.CompanyId == companyId))
            .OrderByDescending(m => m.ModifiedDate ?? m.CreatedDate)
            .Take(count)
            .Select(m => new
            {
                m.ModifiedDate,
                m.CreatedDate,
                m.Status,
                TeacherName = m.Teacher != null ? m.Teacher.FirstName + " " + m.Teacher.LastName : "",
                ParentName = m.Parent != null ? m.Parent.FirstName + " " + m.Parent.LastName : ""
            })
            .ToListAsync();

        return recentMeetings.Select(m => new RecentActivityViewModel
        {
            ActivityDate = m.ModifiedDate ?? m.CreatedDate,
            UserName = m.ParentName,
            ActivityType = GetActivityType(m.Status),
            Description = $"{m.TeacherName} ilə görüş",
            StatusBadgeClass = GetStatusBadgeClass(m.Status),
            StatusText = GetStatusText(m.Status)
        }).ToList();
    }

    /// <summary>
    /// Gələcək görüşlər (bu gün + növbəti 7 gün)
    /// </summary>
    public async Task<List<UpcomingMeetingViewModel>> GetUpcomingMeetingsAsync(Guid? companyId = null, int days = 7)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        var meetings = await _context.Meetings
            .Where(m => !m.IsDeleted &&
                m.MeetingDate >= today &&
                m.MeetingDate <= endDate &&
                (m.Status == MeetingStatus.Pending || m.Status == MeetingStatus.Approved) &&
                (!companyId.HasValue || m.CompanyId == companyId))
            .OrderBy(m => m.MeetingDate)
            .ThenBy(m => m.StartTime)
            .Select(m => new UpcomingMeetingViewModel
            {
                Id = m.Id,
                MeetingDate = m.MeetingDate,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                TeacherName = m.Teacher != null ? m.Teacher.FirstName + " " + m.Teacher.LastName : "",
                ParentName = m.Parent != null ? m.Parent.FirstName + " " + m.Parent.LastName : "",
                StudentName = m.Student != null ? m.Student.FirstName + " " + m.Student.LastName : "",
                Status = m.Status,
                StatusBadgeClass = GetStatusBadgeClass(m.Status)
            })
            .Take(10)
            .ToListAsync();

        return meetings;
    }

    /// <summary>
    /// Top performerlər
    /// </summary>
    public async Task<TopPerformersViewModel> GetTopPerformersAsync(Guid? companyId = null, int topCount = 5)
    {
        var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // Top teachers
        var topTeachers = await _context.Teachers
            .Where(t => t.IsActive && !t.IsDeleted &&
                (!companyId.HasValue || t.CompanyId == companyId))
            .Select(t => new TopTeacherViewModel
            {
                Name = t.FirstName + " " + t.LastName,
                CompletedMeetingsCount = t.Meetings.Count(m =>
                    !m.IsDeleted &&
                    m.Status == MeetingStatus.Completed &&
                    m.MeetingDate >= thisMonthStart),
                TotalStudentsCount = t.TeacherClasses
                    .SelectMany(tc => tc.Class.Students)
                    .Distinct()
                    .Count(s => s.IsActive && !s.IsDeleted)
            })
            .OrderByDescending(t => t.CompletedMeetingsCount)
            .Take(topCount)
            .ToListAsync();

        // Top parents
        var topParents = await _context.Parents
            .Where(p => p.IsActive && !p.IsDeleted &&
                (!companyId.HasValue || p.CompanyId == companyId))
            .Select(p => new TopParentViewModel
            {
                Name = p.FirstName + " " + p.LastName,
                CompletedMeetingsCount = p.Meetings.Count(m =>
                    !m.IsDeleted &&
                    m.Status == MeetingStatus.Completed &&
                    m.MeetingDate >= thisMonthStart),
                ChildrenCount = p.StudentParents.Count(sp =>
                    sp.Student.IsActive && !sp.Student.IsDeleted)
            })
            .OrderByDescending(p => p.CompletedMeetingsCount)
            .Take(topCount)
            .ToListAsync();

        return new TopPerformersViewModel
        {
            TopTeachers = topTeachers,
            TopParents = topParents
        };
    }

    #region Helper Methods

    private int CalculateChangePercent(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (int)Math.Round(((double)(current - previous) / previous) * 100);
    }

    private string GetActivityType(MeetingStatus status)
    {
        return status switch
        {
            MeetingStatus.Pending => "Görüş sorğusu göndərdi",
            MeetingStatus.Approved => "Görüşü təsdiqlədi",
            MeetingStatus.Completed => "Görüş tamamlandı",
            MeetingStatus.Cancelled => "Görüşü ləğv etdi",
            _ => "Fəaliyyət"
        };
    }

    private string GetStatusBadgeClass(MeetingStatus status)
    {
        return status switch
        {
            MeetingStatus.Pending => "bg-warning",
            MeetingStatus.Approved => "bg-primary",
            MeetingStatus.Completed => "bg-success",
            MeetingStatus.Cancelled => "bg-danger",
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
            _ => "Naməlum"
        };
    }

    #endregion
}