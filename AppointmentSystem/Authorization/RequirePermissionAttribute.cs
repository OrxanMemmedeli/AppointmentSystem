using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppointmentSystem.Authorization;

/// <summary>
/// Route-based permission attribute
/// Auto-detect: Route məlumatlarından area/controller/action götürür
/// Manual: [RequirePermission("CustomController", "CustomAction", "Admin")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _controller;
    private readonly string? _action;
    private readonly string? _area;

    /// <summary>
    /// Auto-detect mode - Route-dan məlumatları götürür
    /// </summary>
    public RequirePermissionAttribute()
    {
        // Auto-detect
    }

    /// <summary>
    /// Manual mode - Explicit route təyin edilir
    /// </summary>
    public RequirePermissionAttribute(string controller, string action, string? area = null)
    {
        _controller = controller;
        _action = action;
        _area = area;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get route values (auto-detect if not provided)
        var area = _area ?? context.RouteData.Values["area"]?.ToString() ?? "";
        var controller = _controller ?? context.RouteData.Values["controller"]?.ToString() ?? "";
        var action = _action ?? context.RouteData.Values["action"]?.ToString() ?? "";
        var httpMethod = context.HttpContext.Request.Method;

        if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Create authorization service
        var authorizationService = context.HttpContext.RequestServices
            .GetRequiredService<IAuthorizationService>();

        // Create requirement
        var requirement = new PermissionRequirement(controller, action, area, httpMethod);

        // Check authorization
        var authorizationResult = await authorizationService.AuthorizeAsync(
            context.HttpContext.User,
            null,
            requirement);

        if (!authorizationResult.Succeeded)
        {
            context.Result = new ForbidResult();
        }
    }
}