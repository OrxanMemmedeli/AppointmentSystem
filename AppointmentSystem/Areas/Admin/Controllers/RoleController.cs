using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Role idarəetmə controller - Permission və User əlavəsi daxil
/// </summary>
[Area("Admin")]
public class RoleController : Controller
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IUserService _userService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IRoleService roleService,
        IPermissionService permissionService,
        IUserService userService,
        ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        _userService = userService;
        _logger = logger;
    }

    #region CRUD Actions

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
        return View(new RoleViewModel { IsActive = true });
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

        // Junction table məlumatlarını yüklə
        ViewBag.RolePermissions = await _roleService.GetRolePermissionsAsync(id);
        ViewBag.RoleUsers = await _roleService.GetRoleUsersAsync(id);

        // Dropdown üçün bütün icazələr və istifadəçilər
        ViewBag.AllPermissions = await _permissionService.GetPermissionSelectListAsync();
        ViewBag.AllUsers = await _userService.GetUserSelectListAsync();

        return View(role);
    }

    /// <summary>Rolu yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEditViewBagAsync(model.Id!.Value);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.UpdateRoleAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Rol yenilənərkən xəta baş verdi");
            await LoadEditViewBagAsync(model.Id!.Value);
            return View(model);
        }

        TempData["SuccessMessage"] = "Rol uğurla yeniləndi";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
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

    #endregion

    #region Permission Management

    /// <summary>Rola icazə əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignPermission(Guid roleId, Guid permissionId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.AssignPermissionToRoleAsync(roleId, permissionId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İcazə əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İcazə uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = roleId });
    }

    /// <summary>Roldan icazəni çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePermission(Guid roleId, Guid permissionId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İcazə çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İcazə uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = roleId });
    }

    #endregion

    #region User Management

    /// <summary>Rola istifadəçi əlavə edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUser(Guid roleId, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.AssignUserToRoleAsync(roleId, userId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İstifadəçi əlavə edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İstifadəçi uğurla əlavə edildi";
        }

        return RedirectToAction(nameof(Edit), new { id = roleId });
    }

    /// <summary>Roldan istifadəçini çıxarır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(Guid roleId, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _roleService.RemoveUserFromRoleAsync(roleId, userId, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "İstifadəçi çıxarılarkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "İstifadəçi uğurla çıxarıldı";
        }

        return RedirectToAction(nameof(Edit), new { id = roleId });
    }

    #endregion

    #region Helper Methods

    /// <summary>Edit səhifəsi üçün ViewBag yükləyir</summary>
    private async Task LoadEditViewBagAsync(Guid roleId)
    {
        ViewBag.RolePermissions = await _roleService.GetRolePermissionsAsync(roleId);
        ViewBag.RoleUsers = await _roleService.GetRoleUsersAsync(roleId);
        ViewBag.AllPermissions = await _permissionService.GetPermissionSelectListAsync();
        ViewBag.AllUsers = await _userService.GetUserSelectListAsync();
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}