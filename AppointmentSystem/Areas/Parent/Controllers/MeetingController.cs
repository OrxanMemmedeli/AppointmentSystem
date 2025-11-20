using AppointmentSystem.Data;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Parent.Controllers;

[Area("Parent")]
[Authorize(Policy = "ParentOnly")]
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
    /// Görüşlər siyahısı
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var parentId = GetParentId();
            var companyId = GetCompanyId();

            if (parentId == Guid.Empty)
                return RedirectToAction("Logout", "Auth");

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
    public async Task<IActionResult> Create()
    {
        try
        {
            var parentId = GetParentId();
            var companyId = GetCompanyId();

            if (parentId == Guid.Empty)
                return RedirectToAction("Logout", "Auth");

            // Övladlarını tap
            var children = await _context.StudentParents
                .Include(sp => sp.Student).ThenInclude(s => s.Class)
                .Where(sp => sp.ParentId == parentId)
                .Select(sp => sp.Student)
                .ToListAsync();

            // Müəllimləri tap
            var teachers = await _context.Teachers
                .Where(t => t.CompanyId == companyId && t.IsActive)
                .ToListAsync();

            ViewBag.Children = children;
            ViewBag.Teachers = teachers;

            var model = new CreateMeetingViewModel
            {
                MeetingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yaratma səhifəsi yüklənərkən xəta baş verdi");
            return View("Error");
        }
    }

    /// <summary>
    /// Yeni görüş yaratma POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMeetingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload data
            var parentId = GetParentId();
            var companyId = GetCompanyId();

            var children = await _context.StudentParents
                .Include(sp => sp.Student).ThenInclude(s => s.Class)
                .Where(sp => sp.ParentId == parentId)
                .Select(sp => sp.Student)
                .ToListAsync();

            var teachers = await _context.Teachers
                .Where(t => t.CompanyId == companyId && t.IsActive)
                .ToListAsync();

            ViewBag.Children = children;
            ViewBag.Teachers = teachers;

            return View(model);
        }

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
            return RedirectToAction(nameof(Details), new { id = meetingId });
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
    /// Görüşü ləğv et
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelMeetingViewModel model)
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