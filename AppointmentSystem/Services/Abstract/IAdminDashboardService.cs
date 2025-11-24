using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Admin dashboard məlumatlarını idarə edən servis interfeysi
/// Area-specific: Admin panel statistikaları
/// </summary>
public interface IAdminDashboardService
{
    /// <summary>
    /// Admin dashboard üçün bütün lazımi məlumatları gətirir
    /// </summary>
    /// <param name="companyId">Şirkət ID (multi-tenant filter, null = bütün şirkətlər)</param>
    /// <returns>Dashboard tam data</returns>
    Task<DashboardViewModel> GetDashboardDataAsync(Guid? companyId = null);

    /// <summary>
    /// Statistika kartları (compiled + cached)
    /// </summary>
    /// <param name="companyId">Şirkət ID filter</param>
    Task<DashboardStatisticsViewModel> GetStatisticsAsync(Guid? companyId = null);

    /// <summary>
    /// Aylıq trend məlumatları (son 6 ay) - qrafik üçün
    /// </summary>
    /// <param name="companyId">Şirkət ID filter</param>
    Task<DashboardTrendsViewModel> GetTrendsAsync(Guid? companyId = null);

    /// <summary>
    /// Son fəaliyyətlər (meetings-dən)
    /// </summary>
    /// <param name="companyId">Şirkət ID filter</param>
    /// <param name="count">Nə qədər fəaliyyət qaytarsın</param>
    Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(Guid? companyId = null, int count = 10);

    /// <summary>
    /// Gələcək görüşlər (bu gün + növbəti X gün)
    /// </summary>
    /// <param name="companyId">Şirkət ID filter</param>
    /// <param name="days">Neçə gün əvvəlcədən bax</param>
    Task<List<UpcomingMeetingViewModel>> GetUpcomingMeetingsAsync(Guid? companyId = null, int days = 7);

    /// <summary>
    /// Top performerlər (ən aktiv müəllim və valideynlər - bu ay)
    /// </summary>
    /// <param name="companyId">Şirkət ID filter</param>
    /// <param name="topCount">Top neçə nəfər</param>
    Task<TopPerformersViewModel> GetTopPerformersAsync(Guid? companyId = null, int topCount = 5);
}