using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
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
    private const int CHUNK_SIZE = 100; // Batch insert size

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

            var actionDescriptors = actionDescriptorProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .ToList();

            var discoveredRoutes = new List<DiscoveredRoute>();

            // ✅ AREA OLAN CONTROLLER-LƏR
            var areaNames = actionDescriptors
                .Where(d => d.RouteValues.ContainsKey("area") && !string.IsNullOrEmpty(d.RouteValues["area"]))
                .Select(d => d.RouteValues["area"])
                .Distinct()
                .ToList();

            foreach (var area in areaNames)
            {
                var controllersInArea = actionDescriptors
                    .Where(d => d.RouteValues.ContainsKey("area") &&
                               string.Equals(d.RouteValues["area"], area, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.ControllerName)
                    .Distinct()
                    .ToList();

                foreach (var controller in controllersInArea)
                {
                    var actionsInController = actionDescriptors
                        .Where(d =>
                        {
                            // ✅ NULL-SAFE AREA CHECK
                            var routeArea = d.RouteValues.ContainsKey("area") ? d.RouteValues["area"] : null;
                            return string.Equals(routeArea, area, StringComparison.OrdinalIgnoreCase) &&
                                   string.Equals(d.ControllerName, controller, StringComparison.OrdinalIgnoreCase);
                        })
                        .ToList();

                    foreach (var actionDescriptor in actionsInController)
                    {
                        // [AllowAnonymous] check - controller və ya action level
                        var hasAuthorize = actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null ||
                                          actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null;

                        //// [AllowAnonymous] check
                        //var hasAllowAnonymous = actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null;

                        //// Əgər AllowAnonymous varsa skip et
                        //if (hasAllowAnonymous)
                        //    continue;

                        var action = actionDescriptor.ActionName;
                        var httpMethod = GetHttpMethod(actionDescriptor);

                        discoveredRoutes.Add(new DiscoveredRoute
                        {
                            Area = area!,
                            Controller = controller,
                            Action = action,
                            HttpMethod = httpMethod,
                            HasAuthorize = !hasAuthorize
                        });
                    }
                }
            }

            // ✅ AREA OLMAYAN CONTROLLER-LƏR
            var controllersWithoutArea = actionDescriptors
                .Where(d => !d.RouteValues.ContainsKey("area") || string.IsNullOrEmpty(d.RouteValues["area"]))
                .Select(d => d.ControllerName)
                .Distinct()
                .ToList();

            foreach (var controller in controllersWithoutArea)
            {
                var actionsInController = actionDescriptors
                    .Where(d => (!d.RouteValues.ContainsKey("area") || string.IsNullOrEmpty(d.RouteValues["area"])) &&
                               string.Equals(d.ControllerName, controller, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var actionDescriptor in actionsInController)
                {
                    var hasAuthorize = actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>() != null ||
                                      actionDescriptor.MethodInfo.GetCustomAttribute<AuthorizeAttribute>() != null;

                    var hasAllowAnonymous = actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null;

                    if (hasAllowAnonymous)
                        continue;

                    var action = actionDescriptor.ActionName;
                    var httpMethod = GetHttpMethod(actionDescriptor);

                    discoveredRoutes.Add(new DiscoveredRoute
                    {
                        Area = null,
                        Controller = controller,
                        Action = action,
                        HttpMethod = httpMethod,
                        HasAuthorize = hasAuthorize
                    });
                }
            }

            _logger.LogInformation($"📋 {discoveredRoutes.Count} route tapıldı");

            // ✅ PERMISSION YARATMA VƏ SAVE
            await ProcessAndSavePermissionsAsync(context, discoveredRoutes);

            _logger.LogInformation("✅ Permission seed tamamlandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Permission seed xətası");
            throw;
        }
    }

    /// <summary>
    /// Permission-ları yaradır və DB-yə chunk ilə yazır
    /// </summary>
    private async Task ProcessAndSavePermissionsAsync(
        AppDbContext context,
        List<DiscoveredRoute> discoveredRoutes)
    {

        try
        {
            // Mövcud permission-ları yüklə
            var existingPermissions = await context.Permissions
                .Where(p => p.Type == PermissionType.Action)
                .Select(p => new { p.Code, p.Id, p.IsActive })
                .ToListAsync();

            var existingCodes = existingPermissions.Select(p => p.Code).ToHashSet();

            var permissionsToAdd = new List<Permission>();
            var discoveredCodes = new HashSet<string>();

            foreach (var route in discoveredRoutes)
            {
                var permissionCode = string.IsNullOrEmpty(route.Area)
                    ? $"{route.Controller}.{route.Action}.{route.HttpMethod}".ToUpper()
                    : $"{route.Area}.{route.Controller}.{route.Action}.{route.HttpMethod}".ToUpper();

                discoveredCodes.Add(permissionCode);

                // Əgər artıq mövcuddursa skip et
                if (existingCodes.Contains(permissionCode))
                    continue;

                var resourcePath = string.IsNullOrEmpty(route.Area)
                    ? $"/{route.Controller.ToLower()}/{route.Action.ToLower()}"
                    : $"/{route.Area.ToLower()}/{route.Controller.ToLower()}/{route.Action.ToLower()}";

                var displayName = string.IsNullOrEmpty(route.Area)
                    ? $"{route.Controller}/{route.Action}"
                    : $"{route.Area}/{route.Controller}/{route.Action}";

                var description = $"{route.Controller}Controller - {route.Action} action ({route.HttpMethod})";

                permissionsToAdd.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = displayName,
                    Code = permissionCode,
                    Description = description,
                    ResourcePath = resourcePath,
                    AreaName = route.Area,
                    ControllerName = route.Controller,
                    ActionName = route.Action,
                    HttpMethod = route.HttpMethod,
                    Type = PermissionType.Action,
                    RequiresAuthentication = route.HasAuthorize,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                });
            }

            // ✅ CHUNK INSERT (Batch-lərlə)
            if (permissionsToAdd.Any())
            {
                _logger.LogInformation($"➕ {permissionsToAdd.Count} yeni permission əlavə ediləcək");

                var chunks = permissionsToAdd
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / CHUNK_SIZE)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                foreach (var chunk in chunks)
                {
                    await context.Permissions.AddRangeAsync(chunk);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"   💾 {chunk.Count} permission yazıldı");
                }

                _logger.LogInformation($"✅ Cəmi {permissionsToAdd.Count} yeni permission əlavə edildi");
            }
            else
            {
                _logger.LogInformation("✅ Bütün permissions artıq mövcuddur");
            }

            // ✅ INACTIVE PERMISSION-LARI YENILƏ (artıq mövcud olmayan route-lar)
            var inactivePermissions = existingPermissions
                .Where(p => p.IsActive && !discoveredCodes.Contains(p.Code))
                .Select(p => p.Code)
                .ToList();

            if (inactivePermissions.Any())
            {
                // EF Core 7+ ExecuteUpdate istifadə edək
                var inactiveCount = await context.Permissions
                    .Where(p => inactivePermissions.Contains(p.Code))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.IsActive, false)
                        .SetProperty(p => p.ModifiedDate, DateTime.Now));

                _logger.LogWarning($"⚠️ {inactiveCount} permission deaktiv edildi (route silinib)");
            }
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

        return "GET";
    }

    /// <summary>
    /// Discovered route helper class
    /// </summary>
    private class DiscoveredRoute
    {
        public string? Area { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = "GET";
        public bool HasAuthorize { get; set; }
    }
}