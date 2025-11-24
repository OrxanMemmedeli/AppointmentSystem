using AppointmentSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentSystem.Authorization;

/// <summary>
/// Global permission authorization filter
/// Bütün action-lara avtomatik tətbiq olunur
/// [AllowAnonymous] və SuperAdmin-i bypass edir
/// </summary>
public class GlobalPermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ILogger<GlobalPermissionAuthorizationFilter> _logger;

    public GlobalPermissionAuthorizationFilter(ILogger<GlobalPermissionAuthorizationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // ✅ 1. [AllowAnonymous] check
        if (HasAllowAnonymous(context))
        {
            _logger.LogDebug("✅ [AllowAnonymous] - Access allowed");
            return;
        }

        // ✅ 2. Authentication check
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("🔒 User authenticated deyil");
            context.Result = new ChallengeResult();
            return;
        }

        // ✅ 3. Get UserId
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("🔒 UserId claim tapılmadı");
            context.Result = new ForbidResult();
            return;
        }

        // ✅ 4. Get route values
        var area = context.RouteData.Values["area"]?.ToString() ?? "";
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var action = context.RouteData.Values["action"]?.ToString() ?? "";

        if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
        {
            _logger.LogWarning("🔒 Route məlumatları tapılmadı");
            context.Result = new ForbidResult();
            return;
        }

        // ✅ 5. Get DbContext from DI
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        // ✅ 6. SuperAdmin bypass
        var isSuperAdmin = await dbContext.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId &&
                           ur.Role.Code == "SUPERADMIN" &&
                           ur.Role.IsActive &&
                           !ur.Role.IsDeleted);

        if (isSuperAdmin)
        {
            _logger.LogInformation("✅ SuperAdmin bypass: {Area}/{Controller}/{Action}",
                area, controller, action);
            return;
        }

        // ✅ 7. Route-based permission check
        var hasPermission = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role.IsActive && !ur.Role.IsDeleted)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .AnyAsync(ur => ur.Role.RolePermissions.Any(rp =>
                rp.Permission.IsActive &&
                !rp.Permission.IsDeleted &&
                rp.Permission.Type == Models.Enums.PermissionType.Action &&
                (string.IsNullOrEmpty(area)
                    ? string.IsNullOrEmpty(rp.Permission.AreaName)
                    : rp.Permission.AreaName == area) &&
                rp.Permission.ControllerName == controller &&
                rp.Permission.ActionName == action));

        if (hasPermission)
        {
            _logger.LogInformation("✅ Permission təsdiqləndi: User={UserId}, Route={Area}/{Controller}/{Action}",
                userId, area, controller, action);
            return;
        }

        // ✅ 8. Access denied
        _logger.LogWarning("🔒 Permission rədd edildi: User={UserId}, Route={Area}/{Controller}/{Action}",
            userId, area, controller, action);

        context.Result = new ForbidResult();
    }

    /// <summary>
    /// [AllowAnonymous] attribute-nun olub-olmadığını yoxlayır
    /// </summary>
    private bool HasAllowAnonymous(AuthorizationFilterContext context)
    {
        // Check action descriptor
        var actionDescriptor = context.ActionDescriptor;

        // Check for AllowAnonymousFilter
        if (context.Filters.Any(f => f is IAllowAnonymousFilter))
        {
            return true;
        }

        // Check endpoint metadata
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return true;
        }

        return false;
    }
}