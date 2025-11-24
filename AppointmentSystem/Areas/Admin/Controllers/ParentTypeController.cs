using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Valideyn növü idarəetmə controller
/// </summary>
[Area("Admin")]
public class ParentTypeController : Controller
{
    private readonly IParentTypeService _parentTypeService;
    private readonly ILogger<ParentTypeController> _logger;

    public ParentTypeController(
        IParentTypeService parentTypeService,
        ILogger<ParentTypeController> logger)
    {
        _parentTypeService = parentTypeService;
        _logger = logger;
    }

    /// <summary>Valideyn növü siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var parentTypes = await _parentTypeService.GetAllParentTypesAsync();
        return View(parentTypes);
    }

    /// <summary>Yeni valideyn növü yaratma səhifəsi</summary>
    [HttpGet]
    public IActionResult Create()
    {
        PrepareViewData();
        return View(new ParentTypeViewModel { IsActive = true });
    }

    /// <summary>Yeni valideyn növü yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParentTypeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PrepareViewData();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, parentTypeId) =
            await _parentTypeService.CreateParentTypeAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Valideyn növü yaradılarkən xəta baş verdi");
            PrepareViewData();
            return View(model);
        }

        TempData["SuccessMessage"] = "Valideyn növü uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Valideyn növü redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var parentType = await _parentTypeService.GetParentTypeByIdAsync(id);
        if (parentType == null)
        {
            TempData["ErrorMessage"] = "Valideyn növü tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        PrepareViewData();
        return View(parentType);
    }

    /// <summary>Valideyn növünü yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ParentTypeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PrepareViewData();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentTypeService.UpdateParentTypeAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Valideyn növü yenilənərkən xəta baş verdi");
            PrepareViewData();
            return View(model);
        }

        TempData["SuccessMessage"] = "Valideyn növü uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Valideyn növü statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentTypeService.ToggleParentTypeStatusAsync(id, currentUserId);

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

    /// <summary>Valideyn növü silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var parentType = await _parentTypeService.GetParentTypeByIdAsync(id);
        if (parentType == null)
        {
            TempData["ErrorMessage"] = "Valideyn növü tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(parentType);
    }

    /// <summary>Valideyn növünü silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _parentTypeService.DeleteParentTypeAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Valideyn növü silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Valideyn növü uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>ViewData hazırlayır</summary>
    private void PrepareViewData()
    {
        ViewData["RelationTypes"] = Enum.GetValues(typeof(ParentRelationType))
            .Cast<ParentRelationType>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = GetRelationTypeDisplay(e)
            })
            .ToList();
    }

    /// <summary>Qohumluq növünün göstərilməsi</summary>
    private string GetRelationTypeDisplay(ParentRelationType type)
    {
        return type switch
        {
            ParentRelationType.Father => "Ata",
            ParentRelationType.Mother => "Ana",
            ParentRelationType.Grandfather => "Baba",
            ParentRelationType.Grandmother => "Nənə",
            ParentRelationType.Brother => "Qardaş",
            ParentRelationType.Sister => "Bacı",
            ParentRelationType.Other => "Digər",
            _ => type.ToString()
        };
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
