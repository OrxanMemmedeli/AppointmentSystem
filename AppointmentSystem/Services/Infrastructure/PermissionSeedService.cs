using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppointmentSystem.Services.Infrastructure;

/// <summary>
/// Permission System - Route Scan və DB Sync
/// Application start zamanı bütün route-ları scan edib Permission cədvəlinə yazır
/// </summary>
public class PermissionSeedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSeedService> _logger;

    public PermissionSeedService(
        IServiceProvider serviceProvider,
        ILogger<PermissionSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Bütün route-ları scan edib Permission cədvəlinə yazır
    /// </summary>
    public async Task SeedPermissionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actionDescriptorProvider = scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();

        try
        {
            _logger.LogInformation("🔍 Permission scan başladı...");

            var routes = actionDescriptorProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(x => x.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>() != null ||
                           x.MethodInfo.GetCustomAttribute<AuthorizeAttribute>() != null)
                .ToList();

            var permissionsToAdd = new List<Permission>();
            var existingPermissions = await context.Permissions.ToListAsync();

            foreach (var route in routes)
            {
                var area = route.RouteValues["area"]?.ToString() ?? "";
                var controller = route.RouteValues["controller"]?.ToString() ?? "";
                var action = route.RouteValues["action"]?.ToString() ?? "";

                // HTTP method
                var httpMethod = GetHttpMethod(route);

                // ✅ ResourcePath (lowercase, slash ilə)
                var resourcePath = string.IsNullOrEmpty(area)
                    ? $"/{controller.ToLower()}/{action.ToLower()}"
                    : $"/{area.ToLower()}/{controller.ToLower()}/{action.ToLower()}";

                // Permission kod (readable)
                var permissionCode = string.IsNullOrEmpty(area)
                    ? $"{controller}.{action}".ToUpper()
                    : $"{area}.{controller}.{action}".ToUpper();

                var displayName = string.IsNullOrEmpty(area)
                    ? $"{controller} / {action}"
                    : $"{area} / {controller} / {action}";

                var description = $"{controller}Controller - {action} action ({httpMethod})";

                // Əgər mövcud deyilsə əlavə et
                if (!existingPermissions.Any(p => p.Code == permissionCode))
                {
                    permissionsToAdd.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = displayName,
                        Code = permissionCode,
                        Description = description,
                        ResourcePath = resourcePath, // ✅ /admin/company/create
                        AreaName = area, // ✅ Admin (or empty)
                        ControllerName = controller, // ✅ Company
                        ActionName = action, // ✅ Create
                        HttpMethod = httpMethod,
                        Type = PermissionType.Action,
                        RequiresAuthentication = true,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            if (permissionsToAdd.Any())
            {
                await context.Permissions.AddRangeAsync(permissionsToAdd);
                await context.SaveChangesAsync();
                _logger.LogInformation($"✅ {permissionsToAdd.Count} yeni permission əlavə edildi");
            }
            else
            {
                _logger.LogInformation("✅ Bütün permissions artıq mövcuddur");
            }

            // İnactive olanları yenilə
            await UpdateInactivePermissionsAsync(context, routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Permission seed xətası");
            throw;
        }
    }

    /// <summary>
    /// HTTP method təyin edir
    /// </summary>
    private string GetHttpMethod(ControllerActionDescriptor route)
    {
        var httpMethodAttribute = route.MethodInfo
            .GetCustomAttributes(true)
            .FirstOrDefault(a => a.GetType().Name.StartsWith("Http"));

        if (httpMethodAttribute != null)
        {
            var typeName = httpMethodAttribute.GetType().Name;
            if (typeName.Contains("Get")) return "GET";
            if (typeName.Contains("Post")) return "POST";
            if (typeName.Contains("Put")) return "PUT";
            if (typeName.Contains("Delete")) return "DELETE";
            if (typeName.Contains("Patch")) return "PATCH";
        }

        return "GET"; // Default
    }

    /// <summary>
    /// Artıq mövcud olmayan route-ları deaktiv edir
    /// </summary>
    private async Task UpdateInactivePermissionsAsync(
        AppDbContext context,
        List<ControllerActionDescriptor> currentRoutes)
    {
        var currentPermissionCodes = currentRoutes
            .Select(r => $"{r.RouteValues["area"]}.{r.RouteValues["controller"]}.{r.RouteValues["action"]}".ToUpper())
            .ToHashSet();

        var permissions = await context.Permissions
            .Where(p => p.IsActive && !p.IsDeleted && p.Type == PermissionType.Action)
            .ToListAsync();

        var inactiveCount = 0;
        foreach (var permission in permissions)
        {
            if (!currentPermissionCodes.Contains(permission.Code))
            {
                permission.IsActive = false;
                permission.ModifiedDate = DateTime.Now;
                inactiveCount++;
            }
        }

        if (inactiveCount > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogWarning($"⚠️ {inactiveCount} permission deaktiv edildi (route silinib)");
        }
    }
}