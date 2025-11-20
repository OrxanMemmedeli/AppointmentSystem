using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Policy = "TeacherOnly")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Müəllim Dashboard Ana Səhifə
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            if (!teacherId.HasValue)
            {
                return RedirectToAction("Logout", "Auth");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Statistikalar
            var stats = new
            {
                TodayMeetings = await _context.Meetings
                    .CountAsync(m => m.TeacherId == teacherId && m.MeetingDate == today),

                PendingMeetings = await _context.Meetings
                    .CountAsync(m => m.TeacherId == teacherId && m.Status == MeetingStatus.Pending),

                ApprovedMeetings = await _context.Meetings
                    .CountAsync(m => m.TeacherId == teacherId &&
                                    m.Status == MeetingStatus.Approved &&
                                    m.MeetingDate >= today),

                CompletedThisMonth = await _context.Meetings
                    .CountAsync(m => m.TeacherId == teacherId &&
                                    m.Status == MeetingStatus.Completed &&
                                    m.MeetingDate.Month == DateTime.Today.Month &&
                                    m.MeetingDate.Year == DateTime.Today.Year),

                TotalStudents = await _context.TeacherSubjects
                    .Where(ts => ts.TeacherId == teacherId)
                    .SelectMany(ts => ts.Subject.CompanySubjects)
                    .SelectMany(cs => cs.Company.Students)
                    .Distinct()
                    .CountAsync()
            };

            // Bu günkü görüşlər
            var todayMeetings = await _context.Meetings
                .Include(m => m.Student).ThenInclude(s => s.Class)
                .Include(m => m.Parent)
                .Where(m => m.TeacherId == teacherId && m.MeetingDate == today)
                .OrderBy(m => m.StartTime)
                .Take(5)
                .ToListAsync();

            // Gözləyən görüşlər
            var pendingMeetings = await _context.Meetings
                .Include(m => m.Student).ThenInclude(s => s.Class)
                .Include(m => m.Parent)
                .Where(m => m.TeacherId == teacherId && m.Status == MeetingStatus.Pending)
                .OrderBy(m => m.MeetingDate).ThenBy(m => m.StartTime)
                .Take(5)
                .ToListAsync();

            ViewBag.Stats = stats;
            ViewBag.TodayMeetings = todayMeetings;
            ViewBag.PendingMeetings = pendingMeetings;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim Dashboard yüklənərkən xəta baş verdi");
            return View("Error");
        }
    }
}