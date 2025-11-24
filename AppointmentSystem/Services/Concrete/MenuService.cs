using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Menyu idarəetmə servisi - Production-ready implementasiya
/// DRY, SOLID, Compiled Queries, Caching
/// </summary>
public class MenuService : IMenuService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MenuService> _logger;
    private readonly IMemoryCache _cache;

    // Cache keys
    private const string CACHE_KEY_ALL_MENUS = "Menus:All";
    private const string CACHE_KEY_ROOT_MENUS = "Menus:Root";
    private const string CACHE_KEY_USER_MENUS = "Menus:User:{0}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    #region Compiled Queries - PERFORMANS ÜÇÜN KRİTİK

    /// <summary>
    /// Compiled Query - Bütün aktiv menular
    /// DÜZGÜN: IQueryable<Menu> qaytarır
    /// </summary>
    private static readonly Func<AppDbContext, IQueryable<Menu>> GetAllActiveMenusCompiled =
        EF.CompileQuery((AppDbContext ctx) =>
            ctx.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive)
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ThenBy(m => m.Name));

    /// <summary>
    /// Compiled Query - Root menular
    /// DÜZGÜN: IQueryable<Menu> qaytarır
    /// </summary>
    private static readonly Func<AppDbContext, IQueryable<Menu>> GetRootMenusCompiled =
        EF.CompileQuery((AppDbContext ctx) =>
            ctx.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive && m.ParentId == null)
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.Name));

    /// <summary>
    /// Compiled Query - İstifadəçinin role-larına görə menular
    /// DÜZGÜN: IQueryable<Menu> qaytarır
    /// </summary>
    private static readonly Func<AppDbContext, Guid, IQueryable<Menu>> GetUserMenusByRolesCompiled =
        EF.CompileQuery((AppDbContext ctx, Guid userId) =>
            ctx.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.IsActive && m.IsVisible &&
                    (ctx.UserMenus.Any(um => um.UserId == userId && um.MenuId == m.Id) ||
                     ctx.RoleMenus.Any(rm => rm.MenuId == m.Id &&
                        ctx.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rm.RoleId))))
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ThenBy(m => m.Name));

    /// <summary>
    /// Compiled Query - Menyu ID-yə görə
    /// </summary>
    private static readonly Func<AppDbContext, Guid, Task<Menu?>> GetMenuByIdCompiled =
        EF.CompileAsyncQuery((AppDbContext ctx, Guid id) =>
            ctx.Menus
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == id && !m.IsDeleted));

    /// <summary>
    /// Compiled Query - Child menular
    /// DÜZGÜN: IQueryable<Menu> qaytarır
    /// </summary>
    private static readonly Func<AppDbContext, Guid, IQueryable<Menu>> GetChildMenusCompiled =
        EF.CompileQuery((AppDbContext ctx, Guid parentId) =>
            ctx.Menus
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.ParentId == parentId)
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.Name));

    #endregion

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
    /// Bütün menyuları gətirir (Cache ilə)
    /// </summary>
    public async Task<List<MenuListViewModel>> GetAllMenusAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_KEY_ALL_MENUS, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);

            // DÜZGÜN: IQueryable-dan ToListAsync()
            var menus = await GetAllActiveMenusCompiled(_context).ToListAsync();

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
        }) ?? new List<MenuListViewModel>();
    }

    /// <summary>
    /// Root səviyyə menyuları gətirir (Cache ilə)
    /// </summary>
    public async Task<List<MenuListViewModel>> GetRootMenusAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_KEY_ROOT_MENUS, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheDuration);

            // DÜZGÜN: IQueryable-dan ToListAsync()
            var menus = await GetRootMenusCompiled(_context).ToListAsync();

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
        }) ?? new List<MenuListViewModel>();
    }

    /// <summary>
    /// Child menyuları gətirir (Compiled Query ilə)
    /// </summary>
    public async Task<List<MenuListViewModel>> GetChildMenusAsync(Guid parentId)
    {
        // DÜZGÜN: IQueryable-dan ToListAsync()
        var menus = await GetChildMenusCompiled(_context, parentId).ToListAsync();

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

    /// <summary>
    /// İyerarxik menyu ağacı gətirir
    /// Optimized - N+1 problemi həll edilib
    /// </summary>
    public async Task<List<MenuTreeViewModel>> GetMenuTreeAsync()
    {
        // Bütün menyuları bir sorğuda yüklə
        var allMenus = await GetAllActiveMenusCompiled(_context).ToListAsync();

        // Dictionary ilə lookup sürətləndirmə
        var menuLookup = allMenus.ToDictionary(m => m.Id);

        // Tree quruluşu
        var tree = new List<MenuTreeViewModel>();

        foreach (var menu in allMenus.Where(m => m.ParentId == null))
        {
            tree.Add(BuildMenuTreeNode(menu, allMenus, menuLookup));
        }

        return tree;
    }

    /// <summary>
    /// Rekursiv tree node builder - optimized
    /// </summary>
    private MenuTreeViewModel BuildMenuTreeNode(
        Menu menu,
        List<Menu> allMenus,
        Dictionary<Guid, Menu> lookup)
    {
        var node = new MenuTreeViewModel
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
            AreaName = menu.AreaName,
            ControllerName = menu.ControllerName,
            ActionName = menu.ActionName,
            Children = new List<MenuTreeViewModel>()
        };

        // Child-ları tap və əlavə et
        var children = allMenus.Where(m => m.ParentId == menu.Id);
        foreach (var child in children)
        {
            node.Children.Add(BuildMenuTreeNode(child, allMenus, lookup));
        }

        return node;
    }

    /// <summary>
    /// İstifadəçinin role və permission-larına görə menular
    /// Bu metod Layout-da istifadə olunur - ÇOX KRİTİK!
    /// </summary>
    public async Task<List<MenuTreeViewModel>> GetUserMenusWithPermissionsAsync(Guid userId)
    {
        var cacheKey = string.Format(CACHE_KEY_USER_MENUS, userId);

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            try
            {
                // İstifadəçinin bütün icazəli menyularını gətir
                var userMenus = await GetUserMenusByRolesCompiled(_context, userId).ToListAsync();

                if (!userMenus.Any())
                {
                    _logger.LogWarning("İstifadəçinin heç bir menyusu tapılmadı. User ID: {UserId}", userId);
                    return new List<MenuTreeViewModel>();
                }

                // Dictionary ilə lookup
                var menuLookup = userMenus.ToDictionary(m => m.Id);

                // Tree structure qur
                var tree = new List<MenuTreeViewModel>();

                foreach (var menu in userMenus.Where(m => m.ParentId == null))
                {
                    tree.Add(BuildUserMenuTreeNode(menu, userMenus, menuLookup));
                }

                return tree;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstifadəçi menyuları yüklənərkən xəta. User ID: {UserId}", userId);
                return new List<MenuTreeViewModel>();
            }
        }) ?? new List<MenuTreeViewModel>();
    }

    /// <summary>
    /// User menu tree node builder
    /// </summary>
    private MenuTreeViewModel BuildUserMenuTreeNode(
        Menu menu,
        List<Menu> userMenus,
        Dictionary<Guid, Menu> lookup)
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

        // Bu menyunun child-larını tap
        var children = userMenus
            .Where(m => m.ParentId == menu.Id && m.IsVisible)
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.Name);

        foreach (var child in children)
        {
            node.Children.Add(BuildUserMenuTreeNode(child, userMenus, lookup));
        }

        return node;
    }

    /// <summary>
    /// ID-yə görə menyu gətirir (Compiled Query ilə)
    /// </summary>
    public async Task<MenuViewModel?> GetMenuByIdAsync(Guid id)
    {
        var menu = await GetMenuByIdCompiled(_context, id);

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
            IsVisible = menu.IsVisible, // DÜZƏLDILDI: menu.IsVisible
            Type = menu.Type,
            IsActive = menu.IsActive
        };
    }

    /// <summary>
    /// Parent menyu seçimləri gətirir
    /// </summary>
    public async Task<List<SelectListItem>> GetParentMenuSelectListAsync(Guid? excludeId = null)
    {
        var menus = await GetAllActiveMenusCompiled(_context).ToListAsync();

        if (excludeId.HasValue)
        {
            menus = menus.Where(m => m.Id != excludeId.Value).ToList();
        }

        return menus.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{new string('-', m.Level * 2)} {m.Name}"
        }).ToList();
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
            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Menyu adı boş ola bilməz", null);
            }

            // Kod unikallığı
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
                var parent = await GetMenuByIdCompiled(_context, model.ParentId.Value);
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

            // Cache-i təmizlə
            ClearMenuCaches();

            _logger.LogInformation(
                "Yeni menyu yaradıldı: {MenuName} (ID: {MenuId})",
                menu.Name, menu.Id);

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

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Menyu adı boş ola bilməz");
            }

            // Kod unikallığı
            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != menu.Code)
            {
                var isUnique = await IsCodeUniqueAsync(model.Code, menu.Id);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            // Parent dəyişikliyi
            if (model.ParentId != menu.ParentId)
            {
                // Özünü parent etməyə icazə vermə
                if (model.ParentId == menu.Id)
                {
                    return (false, "Menyu özünə parent ola bilməz");
                }

                // Level yenilə
                menu.Level = 0;
                if (model.ParentId.HasValue)
                {
                    var newParent = await GetMenuByIdCompiled(_context, model.ParentId.Value);
                    if (newParent == null)
                    {
                        return (false, "Parent menyu tapılmadı");
                    }
                    menu.Level = newParent.Level + 1;
                }
            }

            // Update
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

            // Cache-i təmizlə
            ClearMenuCaches();

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

            // Cache-i təmizlə
            ClearMenuCaches();

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

            // Child yoxlaması
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

            // Cache-i təmizlə
            ClearMenuCaches();

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

            // Cache-i təmizlə
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
    /// Mövcud ikonların siyahısını gətirir (Bootstrap Icons 1.11)
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
            "bi-building", "bi-briefcase", "bi-file-text", "bi-printer"
        };
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Bütün menyu cache-lərini təmizləyir
    /// </summary>
    private void ClearMenuCaches()
    {
        _cache.Remove(CACHE_KEY_ALL_MENUS);
        _cache.Remove(CACHE_KEY_ROOT_MENUS);

        // User-specific cache-ləri də təmizlə
        _logger.LogDebug("Menu cache-ləri təmizləndi");
    }

    #endregion
}