using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Dashboard əsas ViewModel
/// </summary>
public class DashboardViewModel
{
    /// <summary>Statistika kartları</summary>
    public DashboardStatisticsViewModel Statistics { get; set; } = new();

    /// <summary>Aylıq trend məlumatları (qrafik üçün)</summary>
    public DashboardTrendsViewModel Trends { get; set; } = new();

    /// <summary>Son fəaliyyətlər</summary>
    public List<RecentActivityViewModel> RecentActivities { get; set; } = new();

    /// <summary>Gözləyən görüşlər (today + upcoming)</summary>
    public List<UpcomingMeetingViewModel> UpcomingMeetings { get; set; } = new();

    /// <summary>Top performerlər</summary>
    public List<TopPerformerViewModel> TopPerformers { get; set; } = new();
}

/// <summary>
/// Dashboard statistika kartları
/// </summary>
public class DashboardStatisticsViewModel
{
    public int TotalCompanies { get; set; }
    public decimal CompaniesChangePercent { get; set; }

    public int TotalTeachers { get; set; }
    public decimal TeachersChangePercent { get; set; }

    public int TotalStudents { get; set; }
    public decimal StudentsChangePercent { get; set; }

    public int TotalParents { get; set; }
    public decimal ParentsChangePercent { get; set; }

    public int TodayMeetings { get; set; }
    public decimal TodayMeetingsChangePercent { get; set; }

    public int PendingMeetings { get; set; }
    public decimal PendingMeetingsChangePercent { get; set; }

    public int CompletedMeetingsThisMonth { get; set; }
    public int CancelledMeetingsThisMonth { get; set; }
}

/// <summary>
/// Aylıq trend məlumatları (Chart.js üçün)
/// </summary>
public class DashboardTrendsViewModel
{
    /// <summary>Son 6 ayın aylıq data-sı</summary>
    public List<MonthlyDataPoint> MonthlyData { get; set; } = new();
}

/// <summary>
/// Aylıq data nöqtəsi
/// </summary>
public class MonthlyDataPoint
{
    public string Month { get; set; } = string.Empty;
    public int TotalMeetings { get; set; }
    public int CompletedMeetings { get; set; }
    public int NewStudents { get; set; }
    public int NewTeachers { get; set; }
}

/// <summary>
/// Son fəaliyyətlər
/// </summary>
public class RecentActivityViewModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "bi-circle";
    public string IconColor { get; set; } = "secondary";
}

/// <summary>
/// Gələcək görüşlər
/// </summary>
public class UpcomingMeetingViewModel
{
    public Guid Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public MeetingStatus Status { get; set; }

    public string StatusBadgeClass => Status switch
    {
        MeetingStatus.Pending => "bg-warning",
        MeetingStatus.Approved => "bg-primary",
        MeetingStatus.Completed => "bg-success",
        MeetingStatus.Cancelled => "bg-danger",
        _ => "bg-secondary"
    };
}

/// <summary>
/// Top performer
/// </summary>
public class TopPerformerViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public int TotalMeetings { get; set; }
    public int CompletedMeetings { get; set; }
    public decimal Rating { get; set; }

    public decimal CompletionRate => TotalMeetings > 0
        ? Math.Round((decimal)CompletedMeetings / TotalMeetings * 100, 1)
        : 0;
}
