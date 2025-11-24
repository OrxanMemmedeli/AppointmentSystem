using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Şirkət/Məktəb idarəetmə controller
/// </summary>
[Area("Admin")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ISubjectService _subjectService;
    private readonly IUserService _userService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(
        ICompanyService companyService,
        ILogger<CompanyController> logger,
        ISubjectService subjectService,
        IUserService userService)
    {
        _companyService = companyService;
        _logger = logger;
        _subjectService = subjectService;
        _userService = userService;
    }

    /// <summary>Şirkət siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var companies = await _companyService.GetAllCompaniesAsync();
        return View(companies);
    }

    /// <summary>Şirkət detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            TempData["ErrorMessage"] = "Şirkət tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        // Junction table məlumatlarını da yüklə
        ViewBag.CompanySubjects = await _companyService.GetCompanySubjectsAsync(id);
        ViewBag.CompanyUsers = await _companyService.GetCompanyUsersAsync(id);


        return View(company);
    }

    /// <summary>Yeni şirkət yaratma səhifəsi</summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CompanyViewModel());
    }

    /// <summary>Yeni şirkət yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, companyId) =
            await _companyService.CreateCompanyAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Şirkət yaradılarkən xəta baş verdi");
            return View(model);
        }

        TempData["SuccessMessage"] = "Şirkət uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Şirkət redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            TempData["ErrorMessage"] = "Şirkət tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        // Junction table məlumatlarını yüklə
        ViewBag.CompanySubjects = await _companyService.GetCompanySubjectsAsync(id);
        ViewBag.CompanyUsers = await _companyService.GetCompanyUsersAsync(id);

        // Dropdown üçün bütün fənləri və istifadəçiləri yüklə
        ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
        ViewBag.AllUsers = await _userService.GetUserSelectListAsync();

        return View(company);
    }

    /// <summary>Şirkəti yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Junction table məlumatlarını yenidən yüklə
            ViewBag.CompanySubjects = await _companyService.GetCompanySubjectsAsync(model.Id.Value);
            ViewBag.CompanyUsers = await _companyService.GetCompanyUsersAsync(model.Id.Value);
            ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
            ViewBag.AllUsers = await _userService.GetUserSelectListAsync();

            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.UpdateCompanyAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Şirkət yenilənərkən xəta baş verdi");

            // Junction table məlumatlarını yenidən yüklə
            ViewBag.CompanySubjects = await _companyService.GetCompanySubjectsAsync(model.Id.Value);
            ViewBag.CompanyUsers = await _companyService.GetCompanyUsersAsync(model.Id.Value);
            ViewBag.AllSubjects = await _subjectService.GetSubjectSelectListAsync();
            ViewBag.AllUsers = await _userService.GetUserSelectListAsync();

            return View(model);
        }

        TempData["SuccessMessage"] = "Şirkət uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Şirkət statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.ToggleCompanyStatusAsync(id, currentUserId);

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

    /// <summary>Şirkəti doğrulayır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.VerifyCompanyAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Doğrulama zamanı xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Şirkət uğurla doğrulandı";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Logo silir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogo(Guid id)
    {
        var (success, errorMessage) = await _companyService.DeleteLogoAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Logo silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Logo uğurla silindi";
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    /// <summary>Background şəkli silir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBackgroundImage(Guid id)
    {
        var (success, errorMessage) = await _companyService.DeleteBackgroundImageAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Background şəkil silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Background şəkil uğurla silindi";
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    /// <summary>Şirkət silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null)
        {
            TempData["ErrorMessage"] = "Şirkət tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(company);
    }

    /// <summary>Şirkəti silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.DeleteCompanyAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Şirkət silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Şirkət uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #region CompanySubject Management (Junction Table)

    /// <summary>Şirkətin fənlərini gətirir (AJAX)</summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanySubjects(Guid companyId)
    {
        var subjects = await _companyService.GetCompanySubjectsAsync(companyId);
        return Json(subjects);
    }

    /// <summary>Şirkətə fənn əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubject(Guid companyId, Guid subjectId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.AssignSubjectToCompanyAsync(companyId, subjectId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Fənn əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Fənn uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = companyId });
    }

    /// <summary>Şirkətdən fənni çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSubject(Guid companyId, Guid subjectId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.RemoveSubjectFromCompanyAsync(companyId, subjectId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Fənn çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Fənn uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = companyId });
    }

    #endregion

    #region CompanyUser Management (Junction Table)

    /// <summary>Şirkətin istifadəçilərini gətirir (AJAX)</summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanyUsers(Guid companyId)
    {
        var users = await _companyService.GetCompanyUsersAsync(companyId);
        return Json(users);
    }

    /// <summary>Şirkətə istifadəçi (manager) əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUser(Guid companyId, Guid userId, bool isManager = false)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.AssignUserToCompanyAsync(companyId, userId, isManager, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İstifadəçi əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İstifadəçi uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = companyId });
    }

    /// <summary>Şirkətdən istifadəçini çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(Guid companyId, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.RemoveUserFromCompanyAsync(companyId, userId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İstifadəçi çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İstifadəçi uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = companyId });
    }

    /// <summary>İstifadəçinin manager statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleManagerStatus(Guid companyId, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.ToggleManagerStatusAsync(companyId, userId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Manager statusu dəyişərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Manager statusu uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Edit), new { id = companyId });
    }

    #endregion


}