using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Müəllim idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,TEACHER_MANAGER")]
public class TeacherController : Controller
{
    private readonly ITeacherService _teacherService;
    private readonly ISubjectService _subjectService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly ISchoolClassService _schoolClassService;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(
        ITeacherService teacherService,
        ISubjectService subjectService,
        IUserService userService,
        ICompanyService companyService,
        ISchoolClassService schoolClassService,
        ILogger<TeacherController> logger)
    {
        _teacherService = teacherService;
        _subjectService = subjectService;
        _userService = userService;
        _companyService = companyService;
        _schoolClassService = schoolClassService;
        _logger = logger;
    }

    #region CRUD Actions

    /// <summary>Müəllim siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? companyId = null)
    {
        List<TeacherListViewModel> teachers;

        if (companyId.HasValue)
        {
            teachers = await _teacherService.GetTeachersByCompanyAsync(companyId.Value);
            ViewBag.FilteredCompanyId = companyId.Value;
        }
        else
        {
            teachers = await _teacherService.GetAllTeachersAsync();
        }

        // Dropdown üçün şirkətləri yüklə
        ViewBag.Companies = await _companyService.GetCompanySelectListAsync();

        return View(teachers);
    }

    /// <summary>Müəllim detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var teacher = await _teacherService.GetTeacherDetailsByIdAsync(id);
        if (teacher == null)
        {
            TempData["ErrorMessage"] = "Müəllim tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(teacher);
    }

    /// <summary>Yeni müəllim yaratma səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Dropdown məlumatlarını yüklə
        ViewBag.Users = await _userService.GetUserSelectListAsync();
        ViewBag.Companies = await _companyService.GetCompanySelectListAsync();

        return View(new TeacherViewModel { IsActive = true });
    }

    /// <summary>Yeni müəllim yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _userService.GetUserSelectListAsync();
            ViewBag.Companies = await _companyService.GetCompanySelectListAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, teacherId) =
            await _teacherService.CreateTeacherAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Müəllim yaradılarkən xəta baş verdi");
            ViewBag.Users = await _userService.GetUserSelectListAsync();
            ViewBag.Companies = await _companyService.GetCompanySelectListAsync();
            return View(model);
        }

        // Şəkil yüklənibsə
        if (model.ImageFile != null && teacherId.HasValue)
        {
            await _teacherService.UploadImageAsync(model.ImageFile, teacherId.Value);
        }

        TempData["SuccessMessage"] = "Müəllim uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Müəllim redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var teacher = await _teacherService.GetTeacherByIdAsync(id);
        if (teacher == null)
        {
            TempData["ErrorMessage"] = "Müəllim tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        // Dropdown məlumatlarını yüklə
        ViewBag.Users = await _userService.GetUserSelectListAsync();
        ViewBag.Companies = await _companyService.GetCompanySelectListAsync();

        // Junction table məlumatlarını yüklə
        ViewBag.TeacherSubjects = await _teacherService.GetTeacherSubjectsAsync(id);
        ViewBag.TeacherClasses = await _teacherService.GetTeacherClassesAsync(id);

        // Yeni əlavə etmək üçün mövcud fənlər və siniflər
        ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
        ViewBag.AllClasses = await _schoolClassService.GetClassSelectListAsync(teacher.CompanyId);

        return View(teacher);
    }

    /// <summary>Müəllimi yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TeacherViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _userService.GetUserSelectListAsync();
            ViewBag.Companies = await _companyService.GetCompanySelectListAsync();
            ViewBag.TeacherSubjects = await _teacherService.GetTeacherSubjectsAsync(model.Id.Value);
            ViewBag.TeacherClasses = await _teacherService.GetTeacherClassesAsync(model.Id.Value);
            ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
            ViewBag.AllClasses = await _schoolClassService.GetClassSelectListAsync(model.CompanyId);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.UpdateTeacherAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Müəllim yenilənərkən xəta baş verdi");
            ViewBag.Users = await _userService.GetUserSelectListAsync();
            ViewBag.Companies = await _companyService.GetCompanySelectListAsync();
            ViewBag.TeacherSubjects = await _teacherService.GetTeacherSubjectsAsync(model.Id.Value);
            ViewBag.TeacherClasses = await _teacherService.GetTeacherClassesAsync(model.Id.Value);
            ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
            ViewBag.AllClasses = await _schoolClassService.GetClassSelectListAsync(model.CompanyId);
            return View(model);
        }

        // Şəkil yüklənibsə
        if (model.ImageFile != null && model.Id.HasValue)
        {
            await _teacherService.UploadImageAsync(model.ImageFile, model.Id.Value);
        }

        TempData["SuccessMessage"] = "Müəllim uğurla yeniləndi";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    /// <summary>Müəllim statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.ToggleTeacherStatusAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Status dəyişərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Status uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Müəllim silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var teacher = await _teacherService.GetTeacherByIdAsync(id);
        if (teacher == null)
        {
            TempData["ErrorMessage"] = "Müəllim tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(teacher);
    }

    /// <summary>Müəllimi silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.DeleteTeacherAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Müəllim silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Müəllim uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Image Management

    /// <summary>Şəkil silir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var (success, errorMessage) = await _teacherService.DeleteImageAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Şəkil silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Şəkil uğurla silindi";
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    #endregion

    #region TeacherSubject Management (Junction Table)

    /// <summary>Müəllimə fənn əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubject(Guid teacherId, Guid subjectId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.AssignSubjectToTeacherAsync(teacherId, subjectId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Fənn əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Fənn uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = teacherId });
    }

    /// <summary>Müəllimdən fənni çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSubject(Guid teacherId, Guid subjectId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.RemoveSubjectFromTeacherAsync(teacherId, subjectId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Fənn çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Fənn uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = teacherId });
    }

    #endregion

    #region TeacherClass Management (Junction Table)

    /// <summary>Müəllimə sinif əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignClass(Guid teacherId, Guid classId, Guid? subjectId, bool isClassLeader = false)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.AssignClassToTeacherAsync(teacherId, classId, subjectId, isClassLeader, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Sinif əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinif uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = teacherId });
    }

    /// <summary>Müəllimdən sinfi çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClass(Guid teacherId, Guid classId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.RemoveClassFromTeacherAsync(teacherId, classId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Sinif çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinif uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = teacherId });
    }

    /// <summary>Sinif rəhbəri statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleClassLeader(Guid teacherId, Guid classId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _teacherService.ToggleClassLeaderAsync(teacherId, classId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Sinif rəhbəri statusu dəyişərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinif rəhbəri statusu uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Edit), new { id = teacherId });
    }

    #endregion

    #region Helper Methods

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}