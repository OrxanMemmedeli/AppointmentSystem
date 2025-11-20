using Application.Services.Interfaces;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Permission idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,PERMISSION_MANAGER")]
public class PermissionController : Controller
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IPermissionService permissionService,
        ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>İcazə siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(PermissionType? type = null)
    {
        List<PermissionListViewModel> permissions;

        if (type.HasValue)
        {
            permissions = await _permissionService.GetPermissionsByTypeAsync(type.Value);
            ViewData["FilterType"] = type.Value;
        }
        else
        {
            permissions = await _permissionService.GetAllPermissionsAsync();
        }

        return View(permissions);
    }

    /// <summary>Yeni icazə yaratma səhifəsi</summary>
    [HttpGet]
    public IActionResult Create()
    {
        PrepareViewData();
        return View(new PermissionViewModel());
    }

    /// <summary>Yeni icazə yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PermissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PrepareViewData();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, permissionId) =
            await _permissionService.CreatePermissionAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "İcazə yaradılarkən xəta baş verdi");
            PrepareViewData();
            return View(model);
        }

        TempData["SuccessMessage"] = "İcazə uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>İcazə redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null)
        {
            TempData["ErrorMessage"] = "İcazə tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        PrepareViewData();
        return View(permission);
    }

    /// <summary>İcazəni yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PermissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PrepareViewData();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _permissionService.UpdatePermissionAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "İcazə yenilənərkən xəta baş verdi");
            PrepareViewData();
            return View(model);
        }

        TempData["SuccessMessage"] = "İcazə uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>İcazə statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _permissionService.TogglePermissionStatusAsync(id, currentUserId);

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

    /// <summary>İcazə silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null)
        {
            TempData["ErrorMessage"] = "İcazə tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(permission);
    }

    /// <summary>İcazəni silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _permissionService.DeletePermissionAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İcazə silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İcazə uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>ViewData hazırlayır</summary>
    private void PrepareViewData()
    {
        ViewData["HttpMethods"] = _permissionService.GetHttpMethods();
        ViewData["PermissionTypes"] = Enum.GetValues<PermissionType>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
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