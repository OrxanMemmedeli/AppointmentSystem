using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Menyu idarəetmə servisi - EF Core 8 uyğun, sadə implementasiya
/// </summary>
public class MenuService : IMenuService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MenuService> _logger;
    private readonly IMemoryCache _cache;

    private const string CACHE_KEY_ALL_MENUS = "Menus:All";
    private const string CACHE_KEY_USER_MENUS = "Menus:User:{0}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public MenuService(
        AppDbContext context,
        ILogger<MenuService> logger,
        IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    #region Query Methods

    /// <summary>
    /// Bütün menyuları gətirir
    /// </summary>
    public async Task<List<MenuListViewModel>> GetAllMenusAsync()
    {
        try
        {
            var menus = await _context.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return menus.Select(m => new MenuListViewModel
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
                CreatedDate = m.CreatedDate,
                AreaName = m.AreaName,
                ControllerName = m.ControllerName,
                ActionName = m.ActionName
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyular yüklənərkən xəta");
            return new List<MenuListViewModel>();
        }
    }

    /// <summary>
    /// Root menyuları gətirir
    /// </summary>
    public async Task<List<MenuListViewModel>> GetRootMenusAsync()
    {
        try
        {
            var menus = await _context.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive && m.ParentId == null)
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return menus.Select(m => new MenuListViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Code = m.Code,
                OrderIndex = m.OrderIndex,
                Level = m.Level,
                IconSVG = m.IconSVG,
                Type = m.Type,
                IsVisible = m.IsVisible,
                IsActive = m.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Root menyular yüklənərkən xəta");
            return new List<MenuListViewModel>();
        }
    }

    /// <summary>
    /// Child menyuları gətirir
    /// </summary>
    public async Task<List<MenuListViewModel>> GetChildMenusAsync(Guid parentId)
    {
        try
        {
            var menus = await _context.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.ParentId == parentId)
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return menus.Select(m => new MenuListViewModel
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
                IsActive = m.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Child menyular yüklənərkən xəta: ParentId={ParentId}", parentId);
            return new List<MenuListViewModel>();
        }
    }

    /// <summary>
    /// Menyu ağacını gətirir
    /// </summary>
    public async Task<List<MenuTreeViewModel>> GetMenuTreeAsync()
    {
        try
        {
            var allMenus = await _context.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive)
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var tree = new List<MenuTreeViewModel>();
            var menuLookup = allMenus.ToDictionary(m => m.Id);

            foreach (var menu in allMenus.Where(m => m.ParentId == null))
            {
                tree.Add(BuildTreeNode(menu, allMenus));
            }

            return tree;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu ağacı yüklənərkən xəta");
            return new List<MenuTreeViewModel>();
        }
    }

    /// <summary>
    /// İstifadəçinin menyularını gətirir (Layout üçün) - KRİTİK METOD
    /// </summary>
    public async Task<List<MenuTreeViewModel>> GetUserMenusWithPermissionsAsync(Guid userId)
    {
        var cacheKey = string.Format(CACHE_KEY_USER_MENUS, userId);

        // Cache-dən yoxla
        if (_cache.TryGetValue(cacheKey, out List<MenuTreeViewModel>? cachedMenus) && cachedMenus != null)
        {
            return cachedMenus;
        }

        try
        {
            // İstifadəçinin rollarını yoxla
            var userRolesCodes = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && !ur.IsDeleted && ur.Role.IsActive)
                .Select(ur => ur.Role.Code)
                .ToListAsync();

            List<Menu> userMenus;

            // SuperAdmin bütün menyuları görür
            if (userRolesCodes.Contains("SUPERADMIN"))
            {
                userMenus = await _context.Menus
                    .AsNoTracking()
                    .Where(m => !m.IsDeleted && m.IsActive && m.IsVisible)
                    .OrderBy(m => m.Level)
                    .ThenBy(m => m.OrderIndex)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            else
            {
                // Role-based menyular - sadələşdirilmiş
                var userRoleIds = await _context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var roleMenuIds = await _context.RoleMenus
                    .AsNoTracking()
                    .Where(rm => userRoleIds.Contains(rm.RoleId) && !rm.IsDeleted)
                    .Select(rm => rm.MenuId)
                    .ToListAsync();

                var userMenuIds = await _context.UserMenus
                    .AsNoTracking()
                    .Where(um => um.UserId == userId && !um.IsDeleted)
                    .Select(um => um.MenuId)
                    .ToListAsync();

                var allMenuIds = roleMenuIds.Union(userMenuIds).Distinct().ToList();

                userMenus = await _context.Menus
                    .AsNoTracking()
                    .Where(m => !m.IsDeleted && m.IsActive && m.IsVisible && allMenuIds.Contains(m.Id))
                    .OrderBy(m => m.Level)
                    .ThenBy(m => m.OrderIndex)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }

            // Tree quruluşu yarat
            var tree = new List<MenuTreeViewModel>();
            foreach (var menu in userMenus.Where(m => m.ParentId == null))
            {
                tree.Add(BuildTreeNode(menu, userMenus));
            }

            // Cache-ə yaz
            _cache.Set(cacheKey, tree, CacheDuration);

            return tree;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstifadəçi menyuları yüklənərkən xəta. UserId: {UserId}", userId);
            return new List<MenuTreeViewModel>();
        }
    }

    /// <summary>
    /// ID-yə görə menyu gətirir
    /// </summary>
    public async Task<MenuViewModel?> GetMenuByIdAsync(Guid id)
    {
        try
        {
            var menu = await _context.Menus
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (menu == null)
                return null;

            return new MenuViewModel
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Name = menu.Name,
                Code = menu.Code,
                Description = menu.Description,
                OrderIndex = menu.OrderIndex,
                Level = menu.Level,
                IconSVG = menu.IconSVG,
                Url = menu.Url,
                AreaName = menu.AreaName,
                ControllerName = menu.ControllerName,
                ActionName = menu.ActionName,
                IsVisible = menu.IsVisible,
                Type = menu.Type,
                IsActive = menu.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu yüklənərkən xəta. Id: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Parent menyu seçimləri gətirir
    /// </summary>
    public async Task<List<SelectListItem>> GetParentMenuSelectListAsync(Guid? excludeId = null)
    {
        try
        {
            var query = _context.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            var menus = await query
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var result = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Ana menyu (yoxdur) --" }
            };

            result.AddRange(menus.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{new string('-', m.Level * 2)} {m.Name}"
            }));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parent menyu siyahısı yüklənərkən xəta");
            return new List<SelectListItem>();
        }
    }

    #endregion

    #region Command Methods

    /// <summary>
    /// Yeni menyu yaradır
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? MenuId)> CreateMenuAsync(
        MenuViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Menyu adı boş ola bilməz", null);
            }

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var isUnique = await IsCodeUniqueAsync(model.Code);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur", null);
                }
            }

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

            var menuId = Guid.NewGuid();
            var menu = new Menu
            {
                Id = menuId,
                ParentId = model.ParentId,
                Name = model.Name.Trim(),
                Code = model.Code?.ToUpperInvariant().Trim(),
                Description = model.Description?.Trim(),
                OrderIndex = model.OrderIndex,
                Level = level,
                IconSVG = model.IconSVG?.Trim() ?? "bi-circle",
                Url = model.Url?.Trim(),
                AreaName = model.AreaName?.Trim(),
                ControllerName = model.ControllerName?.Trim(),
                ActionName = model.ActionName?.Trim(),
                IsVisible = model.IsVisible,
                Type = model.Type,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            ClearMenuCaches();

            _logger.LogInformation("Yeni menyu yaradıldı: {MenuName} (ID: {MenuId})", menu.Name, menu.Id);

            return (true, null, menuId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu yaradılarkən xəta: {MenuName}", model.Name);
            return (false, "Menyu yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>
    /// Menyunu yeniləyir
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(
        MenuViewModel model,
        Guid currentUserId)
    {
        try
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == model.Id && !m.IsDeleted);

            if (menu == null)
            {
                return (false, "Menyu tapılmadı");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Menyu adı boş ola bilməz");
            }

            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != menu.Code)
            {
                var isUnique = await IsCodeUniqueAsync(model.Code, menu.Id);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            if (model.ParentId != menu.ParentId)
            {
                if (model.ParentId == menu.Id)
                {
                    return (false, "Menyu özünə parent ola bilməz");
                }

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
            menu.ModifiedDate = DateTime.Now;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMenuCaches();

            _logger.LogInformation("Menyu yeniləndi: {MenuName} (ID: {MenuId})", menu.Name, menu.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu yenilənərkən xəta: ID {MenuId}", model.Id);
            return (false, "Menyu yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>
    /// Menyu statusunu dəyişir
    /// </summary>
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
            menu.ModifiedDate = DateTime.Now;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMenuCaches();

            _logger.LogInformation("Menyu statusu dəyişdi: {MenuName} - {IsActive}", menu.Name, menu.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu statusu dəyişərkən xəta: ID {MenuId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>
    /// Menyunu silir (Soft Delete)
    /// </summary>
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

            var activeChildCount = menu.Children.Count(c => !c.IsDeleted);
            if (activeChildCount > 0)
            {
                return (false, $"Bu menyunun {activeChildCount} alt menyusu var. Əvvəlcə onları silin.");
            }

            menu.IsDeleted = true;
            menu.IsActive = false;
            menu.ModifiedDate = DateTime.Now;
            menu.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            ClearMenuCaches();

            _logger.LogWarning("Menyu silindi: {MenuName} (ID: {MenuId})", menu.Name, menu.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu silinərkən xəta: ID {MenuId}", id);
            return (false, "Menyu silinərkən xəta baş verdi");
        }
    }

    /// <summary>
    /// Menyu sıralamasını yeniləyir
    /// </summary>
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
                    menu.ModifiedDate = DateTime.Now;
                    menu.ModifiedById = currentUserId;
                }
            }

            await _context.SaveChangesAsync();

            ClearMenuCaches();

            _logger.LogInformation("Menyu sıralaması yeniləndi");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menyu sıralaması yenilənərkən xəta");
            return (false, "Sıralama yenilənərkən xəta baş verdi");
        }
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Kod unikallığını yoxlayır
    /// </summary>
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

    /// <summary>
    /// İkon siyahısı
    /// </summary>
    public List<string> GetAvailableIcons()
    {
        return new List<string>
        {
            "bi-house-door", "bi-speedometer2", "bi-people", "bi-person-badge",
            "bi-mortarboard", "bi-book", "bi-calendar-event", "bi-clipboard-check",
            "bi-folder", "bi-gear", "bi-grid-3x3", "bi-list-ul",
            "bi-envelope", "bi-chat-dots", "bi-bell", "bi-star",
            "bi-heart", "bi-shield-check", "bi-lock", "bi-key",
            "bi-truck", "bi-cart", "bi-credit-card", "bi-wallet",
            "bi-graph-up-arrow", "bi-pie-chart", "bi-bar-chart", "bi-clipboard-data",
            "bi-building", "bi-briefcase", "bi-file-text", "bi-printer",
            "bi-diagram-3", "bi-tag", "bi-link-45deg", "bi-menu-button-wide"
        };
    }

    #endregion

    #region Private Helper Methods

    private MenuTreeViewModel BuildTreeNode(Menu menu, List<Menu> allMenus)
    {
        var node = new MenuTreeViewModel
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            Name = menu.Name,
            Code = menu.Code,
            IconSVG = menu.IconSVG,
            Level = menu.Level,
            OrderIndex = menu.OrderIndex,
            Type = menu.Type,
            IsVisible = menu.IsVisible,
            IsActive = menu.IsActive,
            AreaName = menu.AreaName,
            ControllerName = menu.ControllerName,
            ActionName = menu.ActionName,
            Url = menu.Url,
            Children = new List<MenuTreeViewModel>()
        };

        var children = allMenus
            .Where(m => m.ParentId == menu.Id)
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.Name);

        foreach (var child in children)
        {
            node.Children.Add(BuildTreeNode(child, allMenus));
        }

        return node;
    }

    private void ClearMenuCaches()
    {
        _cache.Remove(CACHE_KEY_ALL_MENUS);
        // User-specific cache-ləri silmək çətindir, ona görə sadəcə timeout-a buraxırıq
        _logger.LogDebug("Menu cache-ləri təmizləndi");
    }

    #endregion
}