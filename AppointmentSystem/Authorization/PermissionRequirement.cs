using AppointmentSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentSystem.Authorization;

/// <summary>
/// Route-based permission requirement
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string? Area { get; }
    public string Controller { get; }
    public string Action { get; }
    public string HttpMethod { get; }

    public PermissionRequirement(string controller, string action, string? area = null, string httpMethod = "GET")
    {
        Area = area;
        Controller = controller;
        Action = action;
        HttpMethod = httpMethod;
    }
}

/// <summary>
/// Route məlumatlarına görə permission yoxlayır
/// Area/Controller/Action kombinasiyası ilə Permission cədvəlinə baxır
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IServiceProvider serviceProvider,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // User authenticated?
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("🔒 Authentication yoxdur");
            context.Fail();
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("🔒 UserId claim tapılmadı");
            context.Fail();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // SuperAdmin bypass
            var isSuperAdmin = await dbContext.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId &&
                               ur.Role.Code == "SUPERADMIN" &&
                               ur.Role.IsActive &&
                               !ur.Role.IsDeleted);

            if (isSuperAdmin)
            {
                _logger.LogInformation("✅ SuperAdmin bypass: {Area}/{Controller}/{Action}",
                    requirement.Area, requirement.Controller, requirement.Action);
                context.Succeed(requirement);
                return;
            }

            // ✅ Route-based permission check
            var hasPermission = await dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.Role.IsActive && !ur.Role.IsDeleted)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .AnyAsync(ur => ur.Role.RolePermissions.Any(rp =>
                    rp.Permission.IsActive &&
                    !rp.Permission.IsDeleted &&
                    rp.Permission.Type == Models.Enums.PermissionType.Action &&
                    (string.IsNullOrEmpty(requirement.Area)
                        ? string.IsNullOrEmpty(rp.Permission.AreaName)
                        : rp.Permission.AreaName == requirement.Area) &&
                    rp.Permission.ControllerName == requirement.Controller &&
                    rp.Permission.ActionName == requirement.Action));

            if (hasPermission)
            {
                _logger.LogInformation("✅ Permission tapıldı: {Area}/{Controller}/{Action}",
                    requirement.Area, requirement.Controller, requirement.Action);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("🔒 Permission tapılmadı: User={UserId}, Route={Area}/{Controller}/{Action}",
                    userId, requirement.Area, requirement.Controller, requirement.Action);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Permission yoxlanarkən xəta");
            context.Fail();
        }
    }
}