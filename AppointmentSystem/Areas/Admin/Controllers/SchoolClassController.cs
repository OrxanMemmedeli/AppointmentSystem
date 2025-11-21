using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Sinif idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,CLASS_MANAGER")]
public class SchoolClassController : Controller
{
    private readonly ISchoolClassService _schoolClassService;
    private readonly ICompanyService _companyService;
    private readonly ILogger<SchoolClassController> _logger;

    public SchoolClassController(
        ISchoolClassService schoolClassService,
        ICompanyService companyService,
        ILogger<SchoolClassController> logger)
    {
        _schoolClassService = schoolClassService;
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>Sinif siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? companyId = null)
    {
        List<SchoolClassListViewModel> classes;

        if (companyId.HasValue)
        {
            classes = await _schoolClassService.GetClassesByCompanyAsync(companyId.Value);
            ViewData["FilterCompanyId"] = companyId.Value;
        }
        else
        {
            classes = await _schoolClassService.GetAllClassesAsync();
        }

        // Şirkət listini dropdown üçün
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();

        return View(classes);
    }

    /// <summary>Sinif detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var classDetails = await _schoolClassService.GetClassDetailsByIdAsync(id);
        if (classDetails == null)
        {
            TempData["ErrorMessage"] = "Sinif tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(classDetails);
    }

    /// <summary>Yeni sinif yaratma səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? companyId = null)
    {
        await PrepareViewDataAsync();

        var model = new SchoolClassViewModel
        {
            CompanyId = companyId ?? Guid.Empty,
            IsActive = true
        };

        return View(model);
    }

    /// <summary>Yeni sinif yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SchoolClassViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, classId) =
            await _schoolClassService.CreateClassAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Sinif yaradılarkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Sinif uğurla yaradıldı";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    /// <summary>Sinif redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var schoolClass = await _schoolClassService.GetClassByIdAsync(id);
        if (schoolClass == null)
        {
            TempData["ErrorMessage"] = "Sinif tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        await PrepareViewDataAsync();
        return View(schoolClass);
    }

    /// <summary>Sinfi yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SchoolClassViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _schoolClassService.UpdateClassAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Sinif yenilənərkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Sinif uğurla yeniləndi";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    /// <summary>Sinif statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _schoolClassService.ToggleClassStatusAsync(id, currentUserId);

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

    /// <summary>Sinif silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var schoolClass = await _schoolClassService.GetClassByIdAsync(id);
        if (schoolClass == null)
        {
            TempData["ErrorMessage"] = "Sinif tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(schoolClass);
    }

    /// <summary>Sinfi silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _schoolClassService.DeleteClassAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Sinif silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinif uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Səviyyəyə görə siniflər (AJAX)</summary>
    [HttpGet]
    public async Task<IActionResult> GetByLevel(int level, Guid? companyId = null)
    {
        var classes = await _schoolClassService.GetClassesByLevelAsync(level, companyId);
        return Json(classes);
    }

    /// <summary>ViewData hazırlayır</summary>
    private async Task PrepareViewDataAsync()
    {
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();
        ViewData["Levels"] = Enumerable.Range(1, 11)
            .Select(i => new SelectListItem
            {
                Value = i.ToString(),
                Text = $"{i}-ci sinif"
            })
            .ToList();
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}