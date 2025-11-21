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
[Authorize(Roles = "ADMIN,COMPANY_MANAGER")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(
        ICompanyService companyService,
        ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _logger = logger;
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

        return View(company);
    }

    /// <summary>Şirkəti yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _companyService.UpdateCompanyAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Şirkət yenilənərkən xəta baş verdi");
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
    [Authorize(Roles = "ADMIN")]
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
}