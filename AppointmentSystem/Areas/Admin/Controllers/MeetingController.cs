using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminViewModels = AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Admin Meeting idarəetmə controller
/// Görüş CRUD əməliyyatları, filtrasiya və idarəetmə
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class MeetingController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly ITeacherService _teacherService;
    private readonly IParentService _parentService;
    private readonly IStudentService _studentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MeetingController> _logger;

    public MeetingController(
        IMeetingService meetingService,
        ITeacherService teacherService,
        IParentService parentService,
        IStudentService studentService,
        ICurrentUserService currentUserService,
        ILogger<MeetingController> logger)
    {
        _meetingService = meetingService;
        _teacherService = teacherService;
        _parentService = parentService;
        _studentService = studentService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region INDEX - List/Filter/Pagination

    /// <summary>
    /// Görüşlərin siyahısı - filtrasiya və pagination ilə
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(AdminViewModels.MeetingFilterViewModel filter)
    {
        try
        {
            var companyId = _currentUserService.CompanyId;
            var result = await _meetingService.GetMeetingsAsync(filter, companyId);

            // Dropdown data
            await LoadFilterDropdownsAsync(companyId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüşlər siyahısı yüklənərkən xəta");
            TempData["Error"] = "Görüşlər yüklənərkən xəta baş verdi";
            return View(new AdminViewModels.PaginatedMeetingListViewModel());
        }
    }

    #endregion

    #region DETAILS

    /// <summary>
    /// Görüş detalları
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(id);

            if (meeting == null)
            {
                TempData["Error"] = "Görüş tapılmadı";
                return RedirectToAction(nameof(Index));
            }

            return View(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş detalları yüklənərkən xəta. ID: {MeetingId}", id);
            TempData["Error"] = "Görüş detalları yüklənərkən xəta baş verdi";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region CREATE

    /// <summary>
    /// Yeni görüş yaratma səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var companyId = _currentUserService.CompanyId;

            var model = new AdminViewModels.MeetingViewModel
            {
                MeetingDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                Status = MeetingStatus.Pending,
                CompanyId = companyId
            };

            await LoadCreateEditDropdownsAsync(companyId);

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yaratma səhifəsi yüklənərkən xəta");
            TempData["Error"] = "Səhifə yüklənərkən xəta baş verdi";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Yeni görüş yaradır
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminViewModels.MeetingViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
                return View(model);
            }

            var currentUserId = _currentUserService.UserId ?? Guid.Empty;
            var (success, errorMessage, meetingId) = await _meetingService.CreateMeetingAsync(model, currentUserId);

            if (!success)
            {
                TempData["Error"] = errorMessage ?? "Görüş yaradılarkən xəta baş verdi";
                await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
                return View(model);
            }

            TempData["Success"] = "Görüş uğurla yaradıldı";
            return RedirectToAction(nameof(Details), new { id = meetingId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yaradılarkən xəta");
            TempData["Error"] = "Görüş yaradılarkən xəta baş verdi";
            await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
            return View(model);
        }
    }

    #endregion

    #region EDIT

    /// <summary>
    /// Görüş redaktə səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var meeting = await _meetingService.GetMeetingForEditAsync(id);

            if (meeting == null)
            {
                TempData["Error"] = "Görüş tapılmadı";
                return RedirectToAction(nameof(Index));
            }

            await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);

            return View(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş redaktə səhifəsi yüklənərkən xəta. ID: {MeetingId}", id);
            TempData["Error"] = "Səhifə yüklənərkən xəta baş verdi";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Görüşü yeniləyir
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminViewModels.MeetingViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
                return View(model);
            }

            var currentUserId = _currentUserService.UserId ?? Guid.Empty;
            var (success, errorMessage) = await _meetingService.UpdateMeetingAsync(model, currentUserId);

            if (!success)
            {
                TempData["Error"] = errorMessage ?? "Görüş yenilənərkən xəta baş verdi";
                await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
                return View(model);
            }

            TempData["Success"] = "Görüş uğurla yeniləndi";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş yenilənərkən xəta. ID: {MeetingId}", model.Id);
            TempData["Error"] = "Görüş yenilənərkən xəta baş verdi";
            await LoadCreateEditDropdownsAsync(_currentUserService.CompanyId);
            return View(model);
        }
    }

    #endregion

    #region DELETE

    /// <summary>
    /// Görüşü silir (soft delete)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? Guid.Empty;
            var (success, errorMessage) = await _meetingService.DeleteMeetingAsync(id, currentUserId);

            if (!success)
            {
                TempData["Error"] = errorMessage ?? "Görüş silinərkən xəta baş verdi";
            }
            else
            {
                TempData["Success"] = "Görüş uğurla silindi";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş silinərkən xəta. ID: {MeetingId}", id);
            TempData["Error"] = "Görüş silinərkən xəta baş verdi";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region COMPLETE

    /// <summary>
    /// Görüşü tamamlanmış kimi işarələyir
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? Guid.Empty;
            var (success, errorMessage) = await _meetingService.CompleteMeetingAsync(id, currentUserId);

            if (!success)
            {
                TempData["Error"] = errorMessage ?? "Görüş tamamlanarkən xəta baş verdi";
            }
            else
            {
                TempData["Success"] = "Görüş uğurla tamamlandı";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüş tamamlanarkən xəta. ID: {MeetingId}", id);
            TempData["Error"] = "Görüş tamamlanarkən xəta baş verdi";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    #endregion

    #region AJAX ENDPOINTS (JSON for dropdowns)

    /// <summary>
    /// Müəllimləri JSON formatında qaytarır (dropdown üçün)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        try
        {
            var companyId = _currentUserService.CompanyId;
            var teachers = await _teacherService.GetActiveTeachersAsync(companyId);

            var result = teachers.Select(t => new
            {
                id = t.Id,
                text = $"{t.FirstName} {t.LastName}"
            });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimlər yüklənərkən xəta");
            return Json(new List<object>());
        }
    }

    /// <summary>
    /// Valideynləri JSON formatında qaytarır (dropdown üçün)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetParents()
    {
        try
        {
            var companyId = _currentUserService.CompanyId;
            var parents = await _parentService.GetActiveParentsAsync(companyId);

            var result = parents.Select(p => new
            {
                id = p.Id,
                text = $"{p.FirstName} {p.LastName}"
            });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideynlər yüklənərkən xəta");
            return Json(new List<object>());
        }
    }

    /// <summary>
    /// Şagirdləri JSON formatında qaytarır (dropdown üçün)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStudents(Guid? parentId = null)
    {
        try
        {
            var companyId = _currentUserService.CompanyId;
            var students = await _studentService.GetActiveStudentsAsync(companyId, parentId);

            var result = students.Select(s => new
            {
                id = s.Id,
                text = $"{s.FirstName} {s.LastName} ({s.Class?.Name ?? "N/A"})"
            });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagirdlər yüklənərkən xəta");
            return Json(new List<object>());
        }
    }

    #endregion

    #region HELPER METHODS

    /// <summary>
    /// Filter üçün dropdown data-ları yükləyir
    /// </summary>
    private async Task LoadFilterDropdownsAsync(Guid? companyId)
    {
        try
        {
            var teachers = await _teacherService.GetActiveTeachersAsync(companyId);
            var parents = await _parentService.GetActiveParentsAsync(companyId);

            ViewBag.Teachers = new SelectList(
                teachers.Select(t => new {
                    Id = t.Id,
                    FullName = $"{t.FirstName} {t.LastName}"
                }),
                "Id",
                "FullName");

            ViewBag.Parents = new SelectList(
                parents.Select(p => new {
                    Id = p.Id,
                    FullName = $"{p.FirstName} {p.LastName}"
                }),
                "Id",
                "FullName");

            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(MeetingStatus))
                .Cast<MeetingStatus>()
                .Select(s => new
                {
                    Value = (int)s,
                    Text = GetStatusText(s)
                }), "Value", "Text");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Filter dropdown-ları yüklənərkən xəta");
        }
    }

    /// <summary>
    /// Create/Edit üçün dropdown data-ları yükləyir
    /// </summary>
    private async Task LoadCreateEditDropdownsAsync(Guid? companyId)
    {
        try
        {
            var teachers = await _teacherService.GetActiveTeachersAsync(companyId);
            var parents = await _parentService.GetActiveParentsAsync(companyId);
            var students = await _studentService.GetActiveStudentsAsync(companyId, null);

            ViewBag.Teachers = new SelectList(
                teachers.Select(t => new {
                    Id = t.Id,
                    FullName = $"{t.FirstName} {t.LastName}"
                }),
                "Id",
                "FullName");

            ViewBag.Parents = new SelectList(
                parents.Select(p => new {
                    Id = p.Id,
                    FullName = $"{p.FirstName} {p.LastName}"
                }),
                "Id",
                "FullName");

            ViewBag.Students = new SelectList(
                students.Select(s => new {
                    Id = s.Id,
                    FullName = $"{s.FirstName} {s.LastName}"
                }),
                "Id",
                "FullName");

            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(MeetingStatus))
                .Cast<MeetingStatus>()
                .Select(s => new
                {
                    Value = (int)s,
                    Text = GetStatusText(s)
                }), "Value", "Text");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create/Edit dropdown-ları yüklənərkən xəta");
        }
    }

    /// <summary>
    /// Status enum-unu Azərbaycan mətnə çevirir
    /// </summary>
    private string GetStatusText(MeetingStatus status)
    {
        return status switch
        {
            MeetingStatus.Pending => "Gözləyir",
            MeetingStatus.Approved => "Təsdiqləndi",
            MeetingStatus.Completed => "Tamamlandı",
            MeetingStatus.Cancelled => "Ləğv edildi",
            MeetingStatus.Declined => "İmtina edildi",
            _ => "Naməlum"
        };
    }

    #endregion
}