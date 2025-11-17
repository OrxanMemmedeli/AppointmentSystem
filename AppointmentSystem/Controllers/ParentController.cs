using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Services;
using AppointmentSystem.Models.ViewModels;
using System.Security.Claims;

namespace AppointmentSystem.Controllers;

/// <summary>
/// Valideyn paneli
/// </summary>
[Authorize(Policy = "ParentOnly")]
public class ParentController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly ILogger<ParentController> _logger;

    public ParentController(
        IMeetingService meetingService,
        ILogger<ParentController> logger)
    {
        _meetingService = meetingService;
        _logger = logger;
    }

    private Guid GetParentId()
    {
        // Real implementation-da Parent entity-dən UserId ilə tapılmalıdır
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : Guid.Empty;
    }

    /// <summary>
    /// Valideyn ana səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var parentId = GetParentId();
            var companyId = GetCompanyId();

            var meetings = await _meetingService.GetParentMeetingsAsync(parentId, companyId);
            return View(meetings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşləri yükləyərkən xəta baş verdi");
            return View("Error");
        }
    }

    /// <summary>
    /// Yeni görüş yaratma səhifəsi
    /// </summary>
    [HttpGet]
    public IActionResult CreateMeeting()
    {
        return View(new CreateMeetingViewModel
        {
            MeetingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        });
    }

    /// <summary>
    /// Yeni görüş yaratma POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMeeting(CreateMeetingViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var parentId = GetParentId();
            var (success, errorMessage, meetingId) = await _meetingService.CreateMeetingAsync(parentId, model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Görüş yaradıla bilmədi");
                return View(model);
            }

            TempData["SuccessMessage"] = "Görüş uğurla yaradıldı. Müəllimin təsdiqi gözlənilir.";
            return RedirectToAction(nameof(MeetingDetails), new { id = meetingId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yaradılarkən xəta baş verdi");
            ModelState.AddModelError(string.Empty, "Xəta baş verdi. Yenidən cəhd edin.");
            return View(model);
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
    /// Görüşü ləğv et
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelMeeting(CancelMeetingViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _meetingService.CancelMeetingAsync(model.MeetingId, model.CancellationReason);

            if (!success)
                return BadRequest("Görüş ləğv edilə bilmədi");

            TempData["SuccessMessage"] = "Görüş uğurla ləğv edildi";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş ləğv edilərkən xəta baş verdi");
            return StatusCode(500, "Xəta baş verdi");
        }
    }

    /// <summary>
    /// Müsait vaxt slotlarını al (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailableTimeSlots(Guid teacherId, DateOnly date)
    {
        try
        {
            var slots = await _meetingService.GetAvailableTimeSlotsAsync(teacherId, date);
            return Json(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vaxt slotlarını yükləyərkən xəta baş verdi");
            return StatusCode(500, "Xəta baş verdi");
        }
    }
}
