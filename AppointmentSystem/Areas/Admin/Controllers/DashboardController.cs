using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext context, 
        ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Admin Dashboard Ana Səhifə
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Dashboard statistikaları
            var stats = new
            {
                TotalCompanies = await _context.Companies.CountAsync(c => c.IsActive),
                TotalTeachers = await _context.Teachers.CountAsync(t => t.IsActive),
                TotalParents = await _context.Parents.CountAsync(p => p.IsActive),
                TotalStudents = await _context.Students.CountAsync(s => s.IsActive),
                TodayMeetings = await _context.Meetings
                    .CountAsync(m => m.MeetingDate == DateOnly.FromDateTime(DateTime.Today)),
                PendingMeetings = await _context.Meetings
                    .CountAsync(m => m.Status == Models.Enums.MeetingStatus.Pending)
            };

            ViewBag.Stats = stats;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin Dashboard yüklənərkən xəta baş verdi");
            return View("Error");
        }
    }
}