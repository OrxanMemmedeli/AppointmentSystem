using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Conrete;

/// <summary>
/// Menu idarəetmə servisi implementasiyası
/// </summary>
public class MenuService : IMenuService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        AppDbContext context,
        ILogger<MenuService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün menyuları gətirir</summary>
    public async Task<List<MenuListViewModel>> GetAllMenusAsync()
    {
        return await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .Select(m => new MenuListViewModel
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                Code = m.Code,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Level = m.Level,
                IconSVG = m.IconSVG,
                Url = m.Url,
                IsVisible = m.IsVisible,
                Type = m.Type,
                IsActive = m.IsActive,
                ChildCount = m.Children.Count(c => !c.IsDeleted),
                ParentName = m.Parent != null ? m.Parent.Name : null,
                CreatedDate = m.CreatedDate
            })
            .OrderBy(m => m.Level)
            .ThenBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>Root səviyyə menyuları gətirir</summary>
    public async Task<List<MenuListViewModel>> GetRootMenusAsync()
    {
        return await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.ParentId == null)
            .Select(m => new MenuListViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Code = m.Code,
                OrderIndex = m.OrderIndex,
                Level = m.Level,
                IconSVG = m.IconSVG,
                Type = m.Type,
                IsVisible = m.IsVisible,
                IsActive = m.IsActive,
                ChildCount = m.Children.Count(c => !c.IsDeleted)
            })
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>Child menyuları gətirir</summary>
    public async Task<List<MenuListViewModel>> GetChildMenusAsync(Guid parentId)
    {
        return await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.ParentId == parentId)
            .Select(m => new MenuListViewModel
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                Code = m.Code,
                OrderIndex = m.OrderIndex,
                Level = m.Level,
                IconSVG = m.IconSVG,
                Type = m.Type,
                IsVisible = m.IsVisible,
                IsActive = m.IsActive,
                ChildCount = m.Children.Count(c => !c.IsDeleted)
            })
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>Iyerarxik menyu ağacı gətirir</summary>
    public async Task<List<MenuTreeViewModel>> GetMenuTreeAsync()
    {
        var allMenus = await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Level)
            .ThenBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .Select(m => new MenuTreeViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Code = m.Code,
                IconSVG = m.IconSVG,
                Level = m.Level,
                OrderIndex = m.OrderIndex,
                Type = m.Type,
                IsVisible = m.IsVisible,
                IsActive = m.IsActive
            })
            .ToListAsync();

        return await BuildMenuTree(allMenus, null);
    }

    /// <summary>Rekursiv menyu ağacı qurur</summary>
    private async Task<List<MenuTreeViewModel>> BuildMenuTree(
        List<MenuTreeViewModel> allMenus,
        Guid? parentId)
    {
        var menus = await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.ParentId == parentId)
            .ToListAsync();

        var result = new List<MenuTreeViewModel>();

        foreach (var menu in menus.OrderBy(m => m.OrderIndex).ThenBy(m => m.Name))
        {
            var treeNode = new MenuTreeViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                Code = menu.Code,
                IconSVG = menu.IconSVG,
                Level = menu.Level,
                OrderIndex = menu.OrderIndex,
                Type = menu.Type,
                IsVisible = menu.IsVisible,
                IsActive = menu.IsActive,
                Children = await BuildMenuTreeRecursiveAsync(menu.Id)
            };

            result.Add(treeNode);
        }

        return result;
    }

    /// <summary>Rekursiv child-ları yükləyir</summary>
    private async Task<List<MenuTreeViewModel>> BuildMenuTreeRecursiveAsync(Guid parentId)
    {
        var children = await _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.ParentId == parentId)
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .ToListAsync();

        var result = new List<MenuTreeViewModel>();

        foreach (var child in children)
        {
            var treeNode = new MenuTreeViewModel
            {
                Id = child.Id,
                Name = child.Name,
                Code = child.Code,
                IconSVG = child.IconSVG,
                Level = child.Level,
                OrderIndex = child.OrderIndex,
                Type = child.Type,
                IsVisible = child.IsVisible,
                IsActive = child.IsActive,
                Children = await BuildMenuTreeRecursiveAsync(child.Id)
            };

            result.Add(treeNode);
        }

        return result;
    }

    /// <summary>ID-yə görə menyu gətirir</summary>
    public async Task<MenuViewModel?> GetMenuByIdAsync(Guid id)
    {
        return await _context.Menus
            .AsNoTracking()
            .Where(m => m.Id == id && !m.IsDeleted)
            .Select(m => new MenuViewModel
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                Code = m.Code,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Level = m.Level,
                IconSVG = m.IconSVG,
                Url = m.Url,
                AreaName = m.AreaName,
                ControllerName = m.ControllerName,
                ActionName = m.ActionName,
                IsVisible = m.IsVisible,
                Type = m.Type,
                IsActive = m.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>Parent menyu seçimləri gətirir</summary>
    public async Task<List<SelectListItem>> GetParentMenuSelectListAsync(Guid? excludeId = null)
    {
        var query = _context.Menus
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.IsActive && m.Type != MenuType.Separator);

        if (excludeId.HasValue)
        {
            // Özünü və child-larını exclude et
            var excludeIds = await GetMenuWithChildrenIdsAsync(excludeId.Value);
            query = query.Where(m => !excludeIds.Contains(m.Id));
        }

        var menus = await query
            .OrderBy(m => m.Level)
            .ThenBy(m => m.OrderIndex)
            .ThenBy(m => m.Name)
            .ToListAsync();

        var result = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Root Menyu" }
        };

        foreach (var menu in menus)
        {
            var prefix = new string('-', menu.Level * 2);
            result.Add(new SelectListItem
            {
                Value = menu.Id.ToString(),
                Text = $"{prefix} {menu.Name}"
            });
        }

        return result;
    }

    /// <summary>Menyunu və bütün child-larının ID-lərini gətirir</summary>
    private async Task<List<Guid>> GetMenuWithChildrenIdsAsync(Guid menuId)
    {
        var ids = new List<Guid> { menuId };

        var childIds = await _context.Menus
            .Where(m => m.ParentId == menuId && !m.IsDeleted)
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var childId in childIds)
        {
            var subChildIds = await GetMenuWithChildrenIdsAsync(childId);
            ids.AddRange(subChildIds);
        }

        return ids;
    }

    /// <summary>Yeni menyu yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? MenuId)> CreateMenuAsync(
        MenuViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Kod unikallığını yoxla
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var isUnique = await IsCodeUniqueAsync(model.Code);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur", null);
                }
            }

            // Level hesabla
            int level = 0;
            if (model.ParentId.HasValue)
            {
                var parent = await _context.Menus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == model.ParentId.Value && !m.IsDeleted);

                if (parent == null)
                {
                    return (false, "Parent menyu tapılmadı", null);
                }

                level = parent.Level + 1;
            }

            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                ParentId = model.ParentId,
                Name = model.Name.Trim(),
                Code = model.Code?.ToUpperInvariant().Trim(),
                Description = model.Description?.Trim(),
                OrderIndex = model.OrderIndex,
                Level = level,
                IconSVG = model.IconSVG?.Trim(),
                Url = model.Url?.Trim(),
                AreaName = model.AreaName?.Trim(),
                ControllerName = model.ControllerName?.Trim(),
                ActionName = model.ActionName?.Trim(),
                IsVisible = model.IsVisible,
                Type = model.Type,
                IsActive = model.IsActive,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedById = currentUserId
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni menyu yaradıldı: {MenuName} (Level: {Level})",
                menu.Name, menu.Level);

            return (true, null, menu.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu yaradılarkən xəta: {MenuName}", model.Name);
            return (false, "Menyu yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Menyunu yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(
        MenuViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Menyu ID-si tələb olunur");
            }

            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == model.Id.Value && !m.IsDeleted);

            if (menu == null)
            {
                return (false, "Menyu tapılmadı");
            }

            // Kod unikallığını yoxla
            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != menu.Code)
            {
                var isUnique = await IsCodeUniqueAsync(model.Code, menu.Id);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            // Parent dəyişikliyi yoxlanması
            if (model.ParentId != menu.ParentId)
            {
                // Özünü parent etməyə icazə vermə
                if (model.ParentId == menu.Id)
                {
                    return (false, "Menyu özünə parent ola bilməz");
                }

                // Child-larını parent etməyə icazə vermə
                var childIds = await GetMenuWithChildrenIdsAsync(menu.Id);
                if (model.ParentId.HasValue && childIds.Contains(model.ParentId.Value))
                {
                    return (false, "Menyu öz child-ına parent ola bilməz");
                }

                // Yeni level hesabla
                menu.Level = 0;
                if (model.ParentId.HasValue)
                {
                    var newParent = await _context.Menus
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == model.ParentId.Value && !m.IsDeleted);

                    if (newParent == null)
                    {
                        return (false, "Parent menyu tapılmadı");
                    }

                    menu.Level = newParent.Level + 1;
                }

                // Bütün child-ların level-ini yenilə
                await UpdateChildrenLevelsAsync(menu.Id, menu.Level);
            }

            menu.ParentId = model.ParentId;
            menu.Name = model.Name.Trim();
            menu.Code = model.Code?.ToUpperInvariant().Trim();
            menu.Description = model.Description?.Trim();
            menu.OrderIndex = model.OrderIndex;
            menu.IconSVG = model.IconSVG?.Trim();
            menu.Url = model.Url?.Trim();
            menu.AreaName = model.AreaName?.Trim();
            menu.ControllerName = model.ControllerName?.Trim();
            menu.ActionName = model.ActionName?.Trim();
            menu.IsVisible = model.IsVisible;
            menu.Type = model.Type;
            menu.IsActive = model.IsActive;
            menu.ModifiedDate = DateTimeOffset.UtcNow;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Menyu yeniləndi: {MenuName} (ID: {MenuId})",
                menu.Name, menu.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu yenilənərkən xəta: ID {MenuId}", model.Id);
            return (false, "Menyu yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Child-ların level-ini rekursiv yeniləyir</summary>
    private async Task UpdateChildrenLevelsAsync(Guid parentId, int parentLevel)
    {
        var children = await _context.Menus
            .Where(m => m.ParentId == parentId && !m.IsDeleted)
            .ToListAsync();

        foreach (var child in children)
        {
            child.Level = parentLevel + 1;
            await UpdateChildrenLevelsAsync(child.Id, child.Level);
        }
    }

    /// <summary>Menyu statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleMenuStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (menu == null)
            {
                return (false, "Menyu tapılmadı");
            }

            menu.IsActive = !menu.IsActive;
            menu.ModifiedDate = DateTimeOffset.UtcNow;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Menyu statusu dəyişdi: {MenuName} - Yeni status: {IsActive}",
                menu.Name, menu.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu statusu dəyişərkən xəta: ID {MenuId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Menyunu silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteMenuAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var menu = await _context.Menus
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (menu == null)
            {
                return (false, "Menyu tapılmadı");
            }

            // Child-ları olan menyunu silmə
            var activeChildCount = menu.Children.Count(c => !c.IsDeleted);
            if (activeChildCount > 0)
            {
                return (false, $"Bu menyunun {activeChildCount} alt menyusu var. Əvvəlcə onları silin.");
            }

            menu.IsDeleted = true;
            menu.IsActive = false;
            menu.ModifiedDate = DateTimeOffset.UtcNow;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Menyu silindi: {MenuName} (ID: {MenuId})",
                menu.Name, menu.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu silinərkən xəta: ID {MenuId}", id);
            return (false, "Menyu silinərkən xəta baş verdi");
        }
    }

    /// <summary>Menyu sıralamasını yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateMenuOrderAsync(
        List<(Guid Id, int OrderIndex)> menuOrders,
        Guid currentUserId)
    {
        try
        {
            foreach (var (id, orderIndex) in menuOrders)
            {
                var menu = await _context.Menus
                    .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

                if (menu != null)
                {
                    menu.OrderIndex = orderIndex;
                    menu.ModifiedDate = DateTimeOffset.UtcNow;
                    menu.ModifiedById = currentUserId;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Menyu sıralaması yeniləndi");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu sıralaması yenilənərkən xəta");
            return (false, "Sıralama yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Kod unikallığını yoxlayır</summary>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var normalizedCode = code.ToUpperInvariant().Trim();

        var query = _context.Menus
            .Where(m => !m.IsDeleted && m.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>İkon siyahısını gətirir (Bootstrap Icons)</summary>
    public List<string> GetAvailableIcons()
    {
        return new List<string>
        {
            "bi-house", "bi-speedometer2", "bi-people", "bi-person",
            "bi-book", "bi-calendar", "bi-clipboard", "bi-file-text",
            "bi-folder", "bi-gear", "bi-grid", "bi-list",
            "bi-envelope", "bi-chat", "bi-bell", "bi-star",
            "bi-heart", "bi-shield", "bi-lock", "bi-key",
            "bi-truck", "bi-cart", "bi-credit-card", "bi-wallet",
            "bi-graph-up", "bi-pie-chart", "bi-bar-chart", "bi-clipboard-data"
        };
    }
}