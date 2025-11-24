using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Valideyn idarəetmə controller
/// </summary>
[Area("Admin")]
public class ParentController : Controller
{
    private readonly IParentService _parentService;
    private readonly ICompanyService _companyService;
    private readonly ILogger<ParentController> _logger;

    public ParentController(
        IParentService parentService,
        ICompanyService companyService,
        ILogger<ParentController> logger)
    {
        _parentService = parentService;
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>Valideyn siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? companyId = null)
    {
        List<ParentListViewModel> parents;

        if (companyId.HasValue)
        {
            parents = await _parentService.GetParentsByCompanyAsync(companyId.Value);
            ViewData["FilterCompanyId"] = companyId.Value;
        }
        else
        {
            parents = await _parentService.GetAllParentsAsync();
        }

        // Şirkət listini dropdown üçün
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();

        return View(parents);
    }

    /// <summary>Valideyn detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var parentDetails = await _parentService.GetParentDetailsByIdAsync(id);
        if (parentDetails == null)
        {
            TempData["ErrorMessage"] = "Valideyn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(parentDetails);
    }

    /// <summary>Yeni valideyn yaratma səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? companyId = null)
    {
        await PrepareViewDataAsync();

        var model = new ParentViewModel
        {
            CompanyId = companyId ?? Guid.Empty,
            IsActive = true
        };

        return View(model);
    }

    /// <summary>Yeni valideyn yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, parentId) =
            await _parentService.CreateParentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Valideyn yaradılarkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Valideyn uğurla yaradıldı";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    /// <summary>Valideyn redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var parent = await _parentService.GetParentByIdAsync(id);
        if (parent == null)
        {
            TempData["ErrorMessage"] = "Valideyn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        await PrepareViewDataAsync();
        return View(parent);
    }

    /// <summary>Valideyni yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ParentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentService.UpdateParentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Valideyn yenilənərkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Valideyn uğurla yeniləndi";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    /// <summary>Valideyn statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentService.ToggleParentStatusAsync(id, currentUserId);

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

    /// <summary>Şəkli silir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var (success, errorMessage) = await _parentService.DeleteImageAsync(id);

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

    /// <summary>Valideyn silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var parent = await _parentService.GetParentByIdAsync(id);
        if (parent == null)
        {
            TempData["ErrorMessage"] = "Valideyn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(parent);
    }

    /// <summary>Valideyni silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentService.DeleteParentAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Valideyn silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Valideyn uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Valideyinin uşaqları (AJAX)</summary>
    [HttpGet]
    public async Task<IActionResult> GetChildren(Guid parentId)
    {
        var children = await _parentService.GetParentChildrenAsync(parentId);
        return Json(children);
    }

    /// <summary>ViewData hazırlayır</summary>
    private async Task PrepareViewDataAsync()
    {
        ViewData["Companies"] = await _companyService.GetCompanySelectListAsync();
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}