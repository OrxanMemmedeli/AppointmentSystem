using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Services.Abstract;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Admin Dashboard Controller
/// Service-based, no direct DB access
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IAdminDashboardService dashboardService,
        ICurrentUserService currentUserService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Admin Dashboard Ana Səhifə
    /// GET: /Admin/Dashboard/Index
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Multi-tenant: Company context-i götür
            var companyId = _currentUserService.CompanyId;

            // Service-dən bütün məlumatları gətir (cached + compiled queries)
            var dashboardData = await _dashboardService.GetDashboardDataAsync(companyId);

            // Breadcrumb
            ViewData["Breadcrumbs"] = new List<(string Text, string? Url)>
            {
                ("Ana Səhifə", Url.Action("Index", "Dashboard")),
                ("Dashboard", null)
            };

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin Dashboard yüklənərkən xəta baş verdi");
            TempData["ErrorMessage"] = "Dashboard yüklənərkən xəta baş verdi. Zəhmət olmasa yenidən cəhd edin.";
            return View(new AppointmentSystem.Areas.Admin.Models.ViewModels.DashboardViewModel());
        }
    }
}