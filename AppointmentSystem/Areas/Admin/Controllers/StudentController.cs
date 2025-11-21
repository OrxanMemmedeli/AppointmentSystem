using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Şagird idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,STUDENT_MANAGER")]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ICompanyService _companyService;
    private readonly ISchoolClassService _schoolClassService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IStudentService studentService,
        ICompanyService companyService,
        ISchoolClassService schoolClassService,
        ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _companyService = companyService;
        _schoolClassService = schoolClassService;
        _logger = logger;
    }

    /// <summary>Şagird siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? companyId = null, Guid? classId = null)
    {
        List<StudentListViewModel> students;

        if (companyId.HasValue)
        {
            students = await _studentService.GetStudentsByCompanyAsync(companyId.Value);
            ViewData["FilterCompanyId"] = companyId.Value;
        }
        else if (classId.HasValue)
        {
            students = await _studentService.GetStudentsByClassAsync(classId.Value);
            ViewData["FilterClassId"] = classId.Value;
        }
        else
        {
            students = await _studentService.GetAllStudentsAsync();
        }

        // Filter dropdown üçün
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();
        ViewData["Classes"] = await _schoolClassService.GetSchoolClassSelectListAsync(companyId);

        return View(students);
    }

    /// <summary>Şagird detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var student = await _studentService.GetStudentDetailsByIdAsync(id);
        if (student == null)
        {
            TempData["ErrorMessage"] = "Şagird tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(student);
    }

    /// <summary>Yeni şagird yaratma səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? companyId = null)
    {
        await PrepareViewDataAsync(companyId);

        var model = new StudentViewModel
        {
            CompanyId = companyId ?? Guid.Empty,
            IsActive = true,
            DateOfBirth = DateTime.Now.AddYears(-7) // Default 7 yaş
        };

        return View(model);
    }

    /// <summary>Yeni şagird yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync(model.CompanyId);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, studentId) =
            await _studentService.CreateStudentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Şagird yaradılarkən xəta baş verdi");
            await PrepareViewDataAsync(model.CompanyId);
            return View(model);
        }

        TempData["SuccessMessage"] = "Şagird uğurla yaradıldı";
        return RedirectToAction(nameof(Details), new { id = studentId });
    }

    /// <summary>Şagird redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null)
        {
            TempData["ErrorMessage"] = "Şagird tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        await PrepareViewDataAsync(student.CompanyId);
        return View(student);
    }

    /// <summary>Şagirdi yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync(model.CompanyId);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentService.UpdateStudentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Şagird yenilənərkən xəta baş verdi");
            await PrepareViewDataAsync(model.CompanyId);
            return View(model);
        }

        TempData["SuccessMessage"] = "Şagird uğurla yeniləndi";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    /// <summary>Şagird statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentService.ToggleStudentStatusAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Status dəyişərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Status uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Şəkli silir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var (success, errorMessage) = await _studentService.DeleteImageAsync(id);

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

    /// <summary>Şagird silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null)
        {
            TempData["ErrorMessage"] = "Şagird tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(student);
    }

    /// <summary>Şagirdi silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentService.DeleteStudentAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Şagird silinərkən xəta baş verdi";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["SuccessMessage"] = "Şagird uğurla silindi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>ViewData hazırlayır</summary>
    private async Task PrepareViewDataAsync(Guid? companyId = null)
    {
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();
        ViewData["Classes"] = await _schoolClassService.GetSchoolClassSelectListAsync(companyId);
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}