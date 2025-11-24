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
    public TopPerformersViewModel TopPerformers { get; set; } = new();
}

/// <summary>
/// Dashboard statistika kartları
/// </summary>
public class DashboardStatisticsViewModel
{
    public int TotalCompanies { get; set; }
    public int CompaniesChangePercent { get; set; } // +5% bu ay

    public int TotalTeachers { get; set; }
    public int TeachersChangePercent { get; set; }

    public int TotalStudents { get; set; }
    public int StudentsChangePercent { get; set; }

    public int TotalParents { get; set; }
    public int ParentsChangePercent { get; set; }

    public int TodayMeetings { get; set; }
    public int TodayMeetingsChangePercent { get; set; }

    public int PendingMeetings { get; set; }
    public int PendingMeetingsChangePercent { get; set; }

    public int CompletedMeetingsThisMonth { get; set; }
    public int CancelledMeetingsThisMonth { get; set; }
}

/// <summary>
/// Aylıq trend məlumatları (Chart.js üçün)
/// </summary>
public class DashboardTrendsViewModel
{
    /// <summary>Son 6 ayın görüş statistikası</summary>
    public List<MonthlyMeetingTrend> MeetingTrends { get; set; } = new();

    /// <summary>Son 6 ayın user artımı</summary>
    public List<MonthlyUserGrowth> UserGrowth { get; set; } = new();
}

public class MonthlyMeetingTrend
{
    public string Month { get; set; } = string.Empty; // "Oktyabr 2024"
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Pending { get; set; }
}

public class MonthlyUserGrowth
{
    public string Month { get; set; } = string.Empty;
    public int Teachers { get; set; }
    public int Students { get; set; }
    public int Parents { get; set; }
}

/// <summary>
/// Son fəaliyyətlər
/// </summary>
public class RecentActivityViewModel
{
    public DateTime ActivityDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty; // "Görüş yaratdı", "Təsdiqləndi" 
    public string Description { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = "bg-secondary"; // Bootstrap class
    public string StatusText { get; set; } = string.Empty;
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
    public string StatusBadgeClass { get; set; } = "bg-secondary";
}

/// <summary>
/// Top performerlər (ən aktiv teacher/parent)
/// </summary>
public class TopPerformersViewModel
{
    public List<TopTeacherViewModel> TopTeachers { get; set; } = new();
    public List<TopParentViewModel> TopParents { get; set; } = new();
}

public class TopTeacherViewModel
{
    public string Name { get; set; } = string.Empty;
    public int CompletedMeetingsCount { get; set; }
    public int TotalStudentsCount { get; set; }
}

public class TopParentViewModel
{
    public string Name { get; set; } = string.Empty;
    public int CompletedMeetingsCount { get; set; }
    public int ChildrenCount { get; set; }
}