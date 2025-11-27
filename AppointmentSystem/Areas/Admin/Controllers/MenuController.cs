using AppointmentSystem.Areas.Admin.Models;
using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Menu idarəetmə controller
/// </summary>
[Area("Admin")]
public class MenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuService menuService,
        ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _logger = logger;
    }

    /// <summary>Menyu siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(string view = "table")
    {
        ViewData["ViewType"] = view;

        if (view == "tree")
        {
            var menuTree = await _menuService.GetMenuTreeAsync();
            return View("IndexTree", menuTree);
        }
        else
        {
            var menus = await _menuService.GetAllMenusAsync();
            return View(menus);
        }
    }

    /// <summary>Yeni menyu yaratma səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? parentId = null)
    {
        await PrepareViewDataAsync(null, parentId);

        var model = new MenuViewModel
        {
            ParentId = parentId,
            OrderIndex = 0,
            IsVisible = true,
            IsActive = true,
            Type = MenuType.Link
        };

        return View(model);
    }

    /// <summary>Yeni menyu yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync(model.Id, model.ParentId);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, menuId) =
            await _menuService.CreateMenuAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Menyu yaradılarkən xəta baş verdi");
            await PrepareViewDataAsync(model.Id, model.ParentId);
            return View(model);
        }

        // ✅ Cache təmizləmə
        _menuService.InvalidateAllMenuCaches();

        TempData["SuccessMessage"] = "Menyu uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Menyu redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id);
        if (menu == null)
        {
            TempData["ErrorMessage"] = "Menyu tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        await PrepareViewDataAsync(menu.Id, menu.ParentId);
        return View(menu);
    }

    /// <summary>Menyunu yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MenuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync(model.Id, model.ParentId);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _menuService.UpdateMenuAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Menyu yenilənərkən xəta baş verdi");
            await PrepareViewDataAsync(model.Id, model.ParentId);
            return View(model);
        }

        // ✅ Cache təmizləmə
        _menuService.InvalidateAllMenuCaches();

        TempData["SuccessMessage"] = "Menyu uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Menyu statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _menuService.ToggleMenuStatusAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Status dəyişərkən xəta baş verdi";
        }
        else
        {
            // ✅ Cache təmizləmə
            _menuService.InvalidateAllMenuCaches();

            TempData["SuccessMessage"] = "Status uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Menyu silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id);
        if (menu == null)
        {
            TempData["ErrorMessage"] = "Menyu tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        // Child sayını əlavə et
        var allMenus = await _menuService.GetAllMenusAsync();
        ViewData["ChildCount"] = allMenus.Count(m => m.ParentId == id);

        return View(menu);
    }

    /// <summary>Menyunu silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _menuService.DeleteMenuAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Menyu silinərkən xəta baş verdi";
        }
        else
        {
            // ✅ Cache təmizləmə
            _menuService.InvalidateAllMenuCaches();

            TempData["SuccessMessage"] = "Menyu uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Menyu sıralamasını yeniləyir (AJAX)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder([FromBody] List<MenuOrderDto> orders)
    {
        if (orders == null || !orders.Any())
        {
            return Json(new { success = false, message = "Sıralama məlumatı tapılmadı" });
        }

        var currentUserId = GetCurrentUserId();
        var menuOrders = orders.Select(o => (o.Id, o.OrderIndex)).ToList();

        var (success, errorMessage) =
            await _menuService.UpdateMenuOrderAsync(menuOrders, currentUserId);

        if (success)
            _menuService.InvalidateAllMenuCaches(); // ✅ Cache təmizləmə

        return Json(new { success, message = errorMessage ?? "Sıralama uğurla yeniləndi" });
    }

    /// <summary>ViewData hazırlayır</summary>
    private async Task PrepareViewDataAsync(Guid? menuId, Guid? currentParentId)
    {
        ViewData["ParentMenus"] = await _menuService.GetParentMenuSelectListAsync(menuId);
        ViewData["MenuTypes"] = Enum.GetValues<MenuType>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
            })
            .ToList();
        ViewData["Icons"] = _menuService.GetAvailableIcons();
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
