using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Parent.Controllers;

[Area("Parent")]
[Authorize(Policy = "ParentOnly")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetParentId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;

        var parent = _context.Parents.FirstOrDefault(p => p.UserId == userId);
        return parent?.Id ?? Guid.Empty;
    }

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : Guid.Empty;
    }

    /// <summary>
    /// Valideyn Dashboard Ana Səhifə
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var parentId = GetParentId();
            var companyId = GetCompanyId();

            if (parentId == Guid.Empty)
            {
                return RedirectToAction("Logout", "Auth");
            }

            var today = DateTime.Today;

            // Statistikalar
            var stats = new
            {
                TotalMeetings = await _context.Meetings
                    .CountAsync(m => m.ParentId == parentId && m.CompanyId == companyId),

                UpcomingMeetings = await _context.Meetings
                    .CountAsync(m => m.ParentId == parentId &&
                                    m.CompanyId == companyId &&
                                    m.MeetingDate >= today &&
                                    (m.Status == MeetingStatus.Pending || m.Status == MeetingStatus.Approved)),

                PendingApproval = await _context.Meetings
                    .CountAsync(m => m.ParentId == parentId &&
                                    m.CompanyId == companyId &&
                                    m.Status == MeetingStatus.Pending),

                CompletedMeetings = await _context.Meetings
                    .CountAsync(m => m.ParentId == parentId &&
                                    m.CompanyId == companyId &&
                                    m.Status == MeetingStatus.Completed),

                MyChildren = await _context.StudentParents
                    .Where(sp => sp.ParentId == parentId)
                    .Select(sp => sp.Student)
                    .Distinct()
                    .CountAsync()
            };

            // Yaxın görüşlər
            var upcomingMeetings = await _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Student).ThenInclude(s => s.Class)
                .Where(m => m.ParentId == parentId &&
                           m.CompanyId == companyId &&
                           m.MeetingDate >= today)
                .OrderBy(m => m.MeetingDate).ThenBy(m => m.StartTime)
                .Take(5)
                .ToListAsync();

            // Son fəaliyyətlər
            var recentActivities = await _context.Meetings
                .Include(m => m.Teacher)
                .Include(m => m.Student)
                .Where(m => m.ParentId == parentId && m.CompanyId == companyId)
                .OrderByDescending(m => m.ModifiedDate ?? m.CreatedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Stats = stats;
            ViewBag.UpcomingMeetings = upcomingMeetings;
            ViewBag.RecentActivities = recentActivities;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn Dashboard yüklənərkən xəta baş verdi");
            return View("Error");
        }
    }
}