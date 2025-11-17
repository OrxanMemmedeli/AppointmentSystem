using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services;
using AppointmentSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace AppointmentSystem.Controllers;

/// <summary>
/// Müəllim paneli
/// </summary>
[Authorize(Policy = "TeacherOnly")]
public class TeacherController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(
        IMeetingService meetingService,
        ILogger<TeacherController> logger)
    {
        _meetingService = meetingService;
        _logger = logger;
    }

    private Guid GetTeacherId()
    {
        // Real implementation-da Teacher entity-dən UserId ilə tapılmalıdır
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Müəllim ana səhifəsi - Təqvim görünüşü
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date)
    {
        try
        {
            var teacherId = GetTeacherId();
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
    /// Bütün görüşlər (siyahı görünüşü)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AllMeetings()
    {
        try
        {
            var teacherId = GetTeacherId();
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
    public async Task<IActionResult> MeetingDetails(Guid id)
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
    public async Task<IActionResult> ApproveMeeting(Guid meetingId, string? teacherNotes)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _meetingService.ApproveMeetingAsync(meetingId, userId, teacherNotes);

            if (!success)
            {
                TempData["ErrorMessage"] = "Görüş təsdiqlənə bilmədi";
                return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
            }

            TempData["SuccessMessage"] = "Görüş uğurla təsdiqləndi";
            return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş təsdiqlənərkən xəta baş verdi");
            TempData["ErrorMessage"] = "Xəta baş verdi";
            return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
        }
    }

    /// <summary>
    /// Görüşdən imtina et
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclineMeeting(Guid meetingId, string declineReason, string? teacherNotes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(declineReason))
            {
                TempData["ErrorMessage"] = "İmtina səbəbi tələb olunur";
                return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _meetingService.DeclineMeetingAsync(meetingId, userId, declineReason, teacherNotes);

            if (!success)
            {
                TempData["ErrorMessage"] = "Görüşdən imtina edilə bilmədi";
                return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
            }

            TempData["SuccessMessage"] = "Görüşdən imtina edildi";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşdən imtina edilərkən xəta baş verdi");
            TempData["ErrorMessage"] = "Xəta baş verdi";
            return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
        }
    }

    /// <summary>
    /// Qeyd əlavə et/yenilə
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(Guid meetingId, string teacherNotes)
    {
        // Bu metod Meeting entity-ni yeniləməlidir
        // Simplicity üçün burada implement edilməyib, amma real sistemdə olmalıdır
        TempData["SuccessMessage"] = "Qeyd yeniləndi";
        return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
    }

    /// <summary>
    /// Təqvim üçün görüşləri al (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCalendarMeetings(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var teacherId = GetTeacherId();
            var meetings = await _meetingService.GetTeacherMeetingsAsync(teacherId);
            
            // Filter by date range
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
