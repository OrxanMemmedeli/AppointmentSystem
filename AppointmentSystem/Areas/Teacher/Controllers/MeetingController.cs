using AppointmentSystem.Data;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Policy = "TeacherOnly")]
public class MeetingController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly AppDbContext _context;
    private readonly ILogger<MeetingController> _logger;

    public MeetingController(
        IMeetingService meetingService,
        AppDbContext context,
        ILogger<MeetingController> logger)
    {
        _meetingService = meetingService;
        _context = context;
        _logger = logger;
    }

    private Guid GetTeacherId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;

        var teacher = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
        return teacher?.Id ?? Guid.Empty;
    }

    /// <summary>
    /// Təqvim görünüşü
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date)
    {
        try
        {
            var teacherId = GetTeacherId();
            if (teacherId == Guid.Empty)
                return RedirectToAction("Logout", "Auth");

            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            var meetings = await _meetingService.GetTeacherMeetingsAsync(teacherId, selectedDate);

            var viewModel = new TeacherCalendarViewModel
            {
                TeacherId = teacherId,
                SelectedDate = selectedDate,
                Meetings = meetings
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşləri yükləyərkən xəta baş verdi");
            return View("Error");
        }
    }

    /// <summary>
    /// Bütün görüşlər
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> All()
    {
        try
        {
            var teacherId = GetTeacherId();
            if (teacherId == Guid.Empty)
                return RedirectToAction("Logout", "Auth");

            var meetings = await _meetingService.GetTeacherMeetingsAsync(teacherId);
            return View(meetings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşləri yükləyərkən xəta baş verdi");
            return View("Error");
        }
    }

    /// <summary>
    /// Görüş detalları
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var meeting = await _meetingService.GetMeetingDetailsAsync(id);

            if (meeting == null)
                return NotFound();

            return View(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş detallarını yükləyərkən xəta baş verdi");
            return View("Error");
        }
    }

    /// <summary>
    /// Görüşü təsdiqlə
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid meetingId, string? teacherNotes)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _meetingService.ApproveMeetingAsync(meetingId, userId, teacherNotes);

            if (!success)
            {
                TempData["ErrorMessage"] = "Görüş təsdiqlənə bilmədi";
                return RedirectToAction(nameof(Details), new { id = meetingId });
            }

            TempData["SuccessMessage"] = "Görüş uğurla təsdiqləndi";
            return RedirectToAction(nameof(Details), new { id = meetingId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş təsdiqlənərkən xəta baş verdi");
            TempData["ErrorMessage"] = "Xəta baş verdi";
            return RedirectToAction(nameof(Details), new { id = meetingId });
        }
    }

    /// <summary>
    /// Görüşdən imtina et
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(Guid meetingId, string declineReason, string? teacherNotes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(declineReason))
            {
                TempData["ErrorMessage"] = "İmtina səbəbi tələb olunur";
                return RedirectToAction(nameof(Details), new { id = meetingId });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _meetingService.DeclineMeetingAsync(meetingId, userId, declineReason, teacherNotes);

            if (!success)
            {
                TempData["ErrorMessage"] = "Görüşdən imtina edilə bilmədi";
                return RedirectToAction(nameof(Details), new { id = meetingId });
            }

            TempData["SuccessMessage"] = "Görüşdən imtina edildi";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşdən imtina edilərkən xəta baş verdi");
            TempData["ErrorMessage"] = "Xəta baş verdi";
            return RedirectToAction(nameof(Details), new { id = meetingId });
        }
    }

    /// <summary>
    /// Təqvim məlumatları (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCalendarData(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var teacherId = GetTeacherId();
            if (teacherId == Guid.Empty)
                return Unauthorized();

            var meetings = await _meetingService.GetTeacherMeetingsAsync(teacherId);

            var filtered = meetings
                .Where(m => m.MeetingDate >= startDate && m.MeetingDate <= endDate)
                .Select(m => new
                {
                    id = m.Id,
                    title = $"{m.StudentName} - {m.ClassName}",
                    start = $"{m.MeetingDate}T{m.StartTime}",
                    end = $"{m.MeetingDate}T{m.EndTime}",
                    status = m.Status.ToString(),
                    backgroundColor = m.Status switch
                    {
                        MeetingStatus.Pending => "#ffc107",
                        MeetingStatus.Approved => "#28a745",
                        MeetingStatus.Declined => "#dc3545",
                        MeetingStatus.Cancelled => "#6c757d",
                        MeetingStatus.Completed => "#17a2b8",
                        _ => "#6c757d"
                    }
                });

            return Json(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Təqvim məlumatlarını yükləyərkən xəta baş verdi");
            return StatusCode(500, "Xəta baş verdi");
        }
    }
}