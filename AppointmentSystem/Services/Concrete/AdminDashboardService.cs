using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Admin dashboard servisi - Debug logging ilə
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly IMemoryCache _cache;

    private const string CACHE_KEY_ADMIN_DASHBOARD = "AdminDashboard:Full:{0}";
    private const string CACHE_KEY_ADMIN_STATISTICS = "AdminDashboard:Statistics:{0}";
    private const string CACHE_KEY_ADMIN_TRENDS = "AdminDashboard:Trends:{0}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

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
    /// Admin dashboard üçün tam məlumatları gətirir
    /// </summary>
    public async Task<DashboardViewModel> GetDashboardDataAsync(Guid? companyId = null)
    {
        // Cache-i bypass edərək test edirik
        try
        {
            _logger.LogInformation("========== DASHBOARD DATA LOADING ==========");
            _logger.LogInformation("CompanyId: {CompanyId}", companyId?.ToString() ?? "NULL (Global)");

            var statistics = await GetStatisticsAsync(companyId);
            var trends = await GetTrendsAsync(companyId);
            var activities = await GetRecentActivitiesAsync(companyId, 10);
            var meetings = await GetUpcomingMeetingsAsync(companyId, 7);
            var performers = await GetTopPerformersAsync(companyId, 5);

            _logger.LogInformation("Statistics loaded: Teachers={Teachers}, Students={Students}, Parents={Parents}",
                statistics.TotalTeachers, statistics.TotalStudents, statistics.TotalParents);

            return new DashboardViewModel
            {
                Statistics = statistics,
                Trends = trends,
                RecentActivities = activities,
                UpcomingMeetings = meetings,
                TopPerformers = performers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard data yüklənərkən xəta");
            return new DashboardViewModel();
        }
    }

    /// <summary>
    /// Statistika kartları - CACHE DISABLED FOR DEBUG
    /// </summary>
    public async Task<DashboardStatisticsViewModel> GetStatisticsAsync(Guid? companyId = null)
    {
        try
        {
            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);

            _logger.LogInformation("===== STATISTICS QUERY START =====");

            // Şirkət sayı
            int totalCompanies;
            if (companyId.HasValue)
            {
                totalCompanies = 1;
            }
            else
            {
                totalCompanies = await _context.Companies
                    .IgnoreQueryFilters() // Global filter-i bypass et
                    .CountAsync(c => c.IsActive && !c.IsDeleted);
                _logger.LogInformation("Companies (raw count, no filter): {Count}", totalCompanies);
            }

            // Müəllimlər - IgnoreQueryFilters ilə test
            var teachersRaw = await _context.Teachers
                .IgnoreQueryFilters()
                .CountAsync(t => !t.IsDeleted);
            _logger.LogInformation("Teachers (raw, IsDeleted=false): {Count}", teachersRaw);

            var teachersFiltered = await _context.Teachers
                .CountAsync(t => t.IsActive && (!companyId.HasValue || t.CompanyId == companyId));
            _logger.LogInformation("Teachers (with company filter): {Count}", teachersFiltered);

            // Şagirdlər
            var studentsRaw = await _context.Students
                .IgnoreQueryFilters()
                .CountAsync(s => !s.IsDeleted);
            _logger.LogInformation("Students (raw): {Count}", studentsRaw);

            var studentsFiltered = await _context.Students
                .CountAsync(s => s.IsActive && (!companyId.HasValue || s.CompanyId == companyId));
            _logger.LogInformation("Students (filtered): {Count}", studentsFiltered);

            // Valideynlər
            var parentsRaw = await _context.Parents
                .IgnoreQueryFilters()
                .CountAsync(p => !p.IsDeleted);
            _logger.LogInformation("Parents (raw): {Count}", parentsRaw);

            var parentsFiltered = await _context.Parents
                .CountAsync(p => p.IsActive && (!companyId.HasValue || p.CompanyId == companyId));
            _logger.LogInformation("Parents (filtered): {Count}", parentsFiltered);

            // Bugünkü görüşlər
            var todayMeetings = await _context.Meetings
                .CountAsync(m => m.MeetingDate == today && (!companyId.HasValue || m.CompanyId == companyId));
            _logger.LogInformation("Today meetings: {Count}", todayMeetings);

            // Gözləyən görüşlər
            var pendingMeetings = await _context.Meetings
                .CountAsync(m => m.Status == MeetingStatus.Pending && (!companyId.HasValue || m.CompanyId == companyId));
            _logger.LogInformation("Pending meetings: {Count}", pendingMeetings);

            // Bu ayın görüşləri
            var completedThisMonth = await _context.Meetings
                .CountAsync(m => m.Status == MeetingStatus.Completed &&
                    m.MeetingDate >= thisMonthStart &&
                    (!companyId.HasValue || m.CompanyId == companyId));

            var cancelledThisMonth = await _context.Meetings
                .CountAsync(m => m.Status == MeetingStatus.Cancelled &&
                    m.MeetingDate >= thisMonthStart &&
                    (!companyId.HasValue || m.CompanyId == companyId));

            _logger.LogInformation("===== STATISTICS QUERY END =====");

            return new DashboardStatisticsViewModel
            {
                TotalCompanies = totalCompanies,
                CompaniesChangePercent = 0,
                TotalTeachers = teachersFiltered,
                TeachersChangePercent = 0,
                TotalStudents = studentsFiltered,
                StudentsChangePercent = 0,
                TotalParents = parentsFiltered,
                ParentsChangePercent = 0,
                TodayMeetings = todayMeetings,
                TodayMeetingsChangePercent = 0,
                PendingMeetings = pendingMeetings,
                PendingMeetingsChangePercent = 0,
                CompletedMeetingsThisMonth = completedThisMonth,
                CancelledMeetingsThisMonth = cancelledThisMonth
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Statistics yüklənərkən xəta");
            return new DashboardStatisticsViewModel();
        }
    }

    /// <summary>
    /// Aylıq trend məlumatları
    /// </summary>
    public async Task<DashboardTrendsViewModel> GetTrendsAsync(Guid? companyId = null)
    {
        try
        {
            var trends = new DashboardTrendsViewModel
            {
                MonthlyData = new List<MonthlyDataPoint>()
            };

            var today = DateTime.Today;

            // Son 6 ayın datası
            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthName = monthStart.ToString("MMM yyyy");

                var meetingsCount = await _context.Meetings
                    .CountAsync(m => m.MeetingDate >= monthStart &&
                        m.MeetingDate <= monthEnd &&
                        (!companyId.HasValue || m.CompanyId == companyId));

                var completedCount = await _context.Meetings
                    .CountAsync(m => m.Status == MeetingStatus.Completed &&
                        m.MeetingDate >= monthStart &&
                        m.MeetingDate <= monthEnd &&
                        (!companyId.HasValue || m.CompanyId == companyId));

                trends.MonthlyData.Add(new MonthlyDataPoint
                {
                    Month = monthName,
                    TotalMeetings = meetingsCount,
                    CompletedMeetings = completedCount,
                    NewStudents = 0,
                    NewTeachers = 0
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trends yüklənərkən xəta");
            return new DashboardTrendsViewModel { MonthlyData = new List<MonthlyDataPoint>() };
        }
    }

    /// <summary>
    /// Son fəaliyyətlər
    /// </summary>
    public async Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(Guid? companyId = null, int count = 10)
    {
        try
        {
            var recentMeetings = await _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Parent)
                .Include(m => m.Student)
                .Where(m => !companyId.HasValue || m.CompanyId == companyId)
                .OrderByDescending(m => m.CreatedDate)
                .Take(count)
                .Select(m => new RecentActivityViewModel
                {
                    Id = m.Id,
                    Type = "Meeting",
                    Title = $"Görüş: {m.Teacher.FirstName} {m.Teacher.LastName}",
                    Description = $"{m.Parent.FirstName} {m.Parent.LastName} ilə görüş planlandı",
                    Timestamp = m.CreatedDate,
                    Icon = "bi-calendar-event",
                    IconColor = m.Status == MeetingStatus.Completed ? "success" :
                               m.Status == MeetingStatus.Cancelled ? "danger" : "primary"
                })
                .ToListAsync();

            return recentMeetings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recent activities yüklənərkən xəta");
            return new List<RecentActivityViewModel>();
        }
    }

    /// <summary>
    /// Gələcək görüşlər
    /// </summary>
    public async Task<List<UpcomingMeetingViewModel>> GetUpcomingMeetingsAsync(Guid? companyId = null, int days = 7)
    {
        try
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(days);

            return await _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Parent)
                .Include(m => m.Student)
                .Where(m => m.MeetingDate >= today &&
                    m.MeetingDate <= endDate &&
                    m.Status == MeetingStatus.Pending &&
                    (!companyId.HasValue || m.CompanyId == companyId))
                .OrderBy(m => m.MeetingDate)
                .ThenBy(m => m.StartTime)
                .Take(10)
                .Select(m => new UpcomingMeetingViewModel
                {
                    Id = m.Id,
                    TeacherName = $"{m.Teacher.FirstName} {m.Teacher.LastName}",
                    ParentName = $"{m.Parent.FirstName} {m.Parent.LastName}",
                    StudentName = m.Student != null ? $"{m.Student.FirstName} {m.Student.LastName}" : "-",
                    MeetingDate = m.MeetingDate,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Status = m.Status
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upcoming meetings yüklənərkən xəta");
            return new List<UpcomingMeetingViewModel>();
        }
    }

    /// <summary>
    /// Top performerlər
    /// </summary>
    public async Task<List<TopPerformerViewModel>> GetTopPerformersAsync(Guid? companyId = null, int topCount = 5)
    {
        try
        {
            var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            return await _context.Teachers
                .Where(t => t.IsActive && (!companyId.HasValue || t.CompanyId == companyId))
                .Select(t => new TopPerformerViewModel
                {
                    Id = t.Id,
                    Name = $"{t.FirstName} {t.LastName}",
                    ProfileImage = t.ImagePath,
                    TotalMeetings = t.Meetings.Count(m => m.MeetingDate >= thisMonthStart),
                    CompletedMeetings = t.Meetings.Count(m => m.Status == MeetingStatus.Completed && m.MeetingDate >= thisMonthStart),
                    Rating = 4.5m // Placeholder
                })
                .OrderByDescending(t => t.CompletedMeetings)
                .Take(topCount)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Top performers yüklənərkən xəta");
            return new List<TopPerformerViewModel>();
        }
    }

    private static decimal CalculateChangePercent(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((decimal)(current - previous) / previous * 100, 1);
    }
}
