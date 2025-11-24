using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Data;
using AppointmentSystem.Services.Abstract;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Cari istifadəçi məlumatları servisi
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private readonly ILogger<CurrentUserService> _logger;

    // Cache üçün
    private Guid? _cachedTeacherId;
    private Guid? _cachedParentId;
    private List<string>? _cachedPermissionCodes;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _logger = logger;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FullName => _httpContextAccessor.HttpContext?.User?.FindFirst("FullName")?.Value;

    public Guid? CompanyId
    {
        get
        {
            var companyIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
            return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public IEnumerable<string> GetRoles()
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
               ?? Enumerable.Empty<string>();
    }

    #region ROUTE-BASED PERMISSION METHODS

    /// <summary>
    /// Route məlumatlarına görə permission yoxlayır
    /// </summary>
    public async Task<bool> HasPermissionAsync(string controller, string action, string? area = null)
    {
        if (!UserId.HasValue)
            return false;

        try
        {
            // SuperAdmin bypass
            var isSuperAdmin = await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == UserId.Value &&
                               ur.Role.Code == "SUPERADMIN" &&
                               ur.Role.IsActive &&
                               !ur.Role.IsDeleted);

            if (isSuperAdmin)
                return true;

            // Route-based check
            var hasPermission = await _context.UserRoles
                .Where(ur => ur.UserId == UserId.Value && ur.Role.IsActive && !ur.Role.IsDeleted)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .AnyAsync(ur => ur.Role.RolePermissions.Any(rp =>
                    rp.Permission.IsActive &&
                    !rp.Permission.IsDeleted &&
                    rp.Permission.Type == PermissionType.Action &&
                    (string.IsNullOrEmpty(area)
                        ? string.IsNullOrEmpty(rp.Permission.AreaName)
                        : rp.Permission.AreaName == area) &&
                    rp.Permission.ControllerName == controller &&
                    rp.Permission.ActionName == action));

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission yoxlanarkən xəta. Route: {Area}/{Controller}/{Action}",
                area, controller, action);
            return false;
        }
    }

    /// <summary>
    /// Permission kod siyahısını gətirir (UI üçün, cache ilə)
    /// </summary>
    public async Task<List<string>> GetPermissionCodesAsync()
    {
        if (_cachedPermissionCodes != null)
            return _cachedPermissionCodes;

        if (!UserId.HasValue)
            return new List<string>();

        try
        {
            // SuperAdmin bypass
            var isSuperAdmin = await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == UserId.Value &&
                               ur.Role.Code == "SUPERADMIN" &&
                               ur.Role.IsActive);

            if (isSuperAdmin)
            {
                // ✅ Nullable filter
                _cachedPermissionCodes = await _context.Permissions
                    .Where(p => p.IsActive && !p.IsDeleted && p.Code != null)
                    .Select(p => p.Code!)
                    .ToListAsync();

                return _cachedPermissionCodes;
            }

            // ✅ Role-based permissions - nullable filter
            _cachedPermissionCodes = await _context.UserRoles
                .Where(ur => ur.UserId == UserId.Value)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .SelectMany(ur => ur.Role.RolePermissions
                    .Where(rp => rp.Permission.IsActive &&
                                !rp.Permission.IsDeleted &&
                                rp.Permission.Code != null)
                    .Select(rp => rp.Permission.Code!))
                .Distinct()
                .ToListAsync();

            return _cachedPermissionCodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission kodları yüklənərkən xəta");
            return new List<string>();
        }
    }

    #endregion

    #region HELPER METHODS

    /// <summary>
    /// Teacher ID-ni qaytarır (cache ilə)
    /// </summary>
    public async Task<Guid?> GetTeacherIdAsync()
    {
        if (_cachedTeacherId.HasValue)
            return _cachedTeacherId;

        if (!UserId.HasValue)
            return null;

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.UserId == UserId.Value && t.IsActive);

        _cachedTeacherId = teacher?.Id;
        return _cachedTeacherId;
    }

    /// <summary>
    /// Parent ID-ni qaytarır (cache ilə)
    /// </summary>
    public async Task<Guid?> GetParentIdAsync()
    {
        if (_cachedParentId.HasValue)
            return _cachedParentId;

        if (!UserId.HasValue)
            return null;

        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == UserId.Value && p.IsActive);

        _cachedParentId = parent?.Id;
        return _cachedParentId;
    }

    #endregion
}