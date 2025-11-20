using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Role idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,ROLE_MANAGER")]
public class RoleController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IRoleService roleService,
        ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>Rol siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return View(roles);
    }

    /// <summary>Yeni rol yaratma səhifəsi</summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoleViewModel());
    }

    /// <summary>Yeni rol yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, roleId) = await _roleService.CreateRoleAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Rol yaradılarkən xəta baş verdi");
            return View(model);
        }

        TempData["SuccessMessage"] = "Rol uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Rol redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Rol tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(role);
    }

    /// <summary>Rolu yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.UpdateRoleAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Rol yenilənərkən xəta baş verdi");
            return View(model);
        }

        TempData["SuccessMessage"] = "Rol uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Rol statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.ToggleRoleStatusAsync(id, currentUserId);

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

    /// <summary>Rol silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Rol tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(role);
    }

    /// <summary>Rolu silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.DeleteRoleAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Rol silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Rol uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}