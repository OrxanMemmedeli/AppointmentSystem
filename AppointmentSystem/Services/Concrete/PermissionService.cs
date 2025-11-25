using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Permission idarəetmə servisi implementasiyası
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        AppDbContext context,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün icazələri gətirir</summary>
    public async Task<List<PermissionListViewModel>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new PermissionListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                ResourcePath = p.ResourcePath,
                HttpMethod = p.HttpMethod,
                Type = p.Type,
                RequiresAuthentication = p.RequiresAuthentication,
                IsActive = p.IsActive,
                RoleCount = p.RolePermissions.Count(rp => !rp.IsDeleted),
                UserCount = p.UserPermissions.Count(up => !up.IsDeleted),
                CreatedDate = p.CreatedDate
            })
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>Aktiv icazələri gətirir</summary>
    public async Task<List<PermissionListViewModel>> GetActivePermissionsAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .Select(p => new PermissionListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Type = p.Type,
                IsActive = p.IsActive
            })
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>Tipə görə icazələri gətirir</summary>
    public async Task<List<PermissionListViewModel>> GetPermissionsByTypeAsync(PermissionType type)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Type == type)
            .Select(p => new PermissionListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Type = p.Type,
                IsActive = p.IsActive
            })
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>ID-yə görə icazə gətirir</summary>
    public async Task<PermissionViewModel?> GetPermissionByIdAsync(Guid id)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new PermissionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                ResourcePath = p.ResourcePath,
                AreaName = p.AreaName,
                ControllerName = p.ControllerName,
                ActionName = p.ActionName,
                HttpMethod = p.HttpMethod,
                Type = p.Type,
                RequiresAuthentication = p.RequiresAuthentication,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>Yeni icazə yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? PermissionId)> CreatePermissionAsync(
        PermissionViewModel model,
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

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Code = model.Code?.ToUpperInvariant().Trim(),
                Description = model.Description?.Trim(),
                ResourcePath = model.ResourcePath?.Trim(),
                AreaName = model.AreaName?.Trim(),
                ControllerName = model.ControllerName?.Trim(),
                ActionName = model.ActionName?.Trim(),
                HttpMethod = model.HttpMethod.ToUpperInvariant(),
                Type = model.Type,
                RequiresAuthentication = model.RequiresAuthentication,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni icazə yaradıldı: {PermissionName} (ID: {PermissionId})",
                permission.Name, permission.Id);

            return (true, null, permission.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İcazə yaradılarkən xəta: {PermissionName}", model.Name);
            return (false, "İcazə yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>İcazənin məlumatlarını yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdatePermissionAsync(
        PermissionViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "İcazə ID-si tələb olunur");
            }

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == model.Id.Value && !p.IsDeleted);

            if (permission == null)
            {
                return (false, "İcazə tapılmadı");
            }

            // Kod unikallığını yoxla
            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != permission.Code)
            {
                var isUnique = await IsCodeUniqueAsync(model.Code, permission.Id);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            permission.Name = model.Name.Trim();
            permission.Code = model.Code?.ToUpperInvariant().Trim();
            permission.Description = model.Description?.Trim();
            permission.ResourcePath = model.ResourcePath?.Trim();
            permission.AreaName = model.AreaName?.Trim();
            permission.ControllerName = model.ControllerName?.Trim();
            permission.ActionName = model.ActionName?.Trim();
            permission.HttpMethod = model.HttpMethod.ToUpperInvariant();
            permission.Type = model.Type;
            permission.RequiresAuthentication = model.RequiresAuthentication;
            permission.IsActive = model.IsActive;
            permission.ModifiedDate = DateTime.Now;
            permission.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "İcazə yeniləndi: {PermissionName} (ID: {PermissionId})",
                permission.Name, permission.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İcazə yenilənərkən xəta: ID {PermissionId}", model.Id);
            return (false, "İcazə yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>İcazənin statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> TogglePermissionStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (permission == null)
            {
                return (false, "İcazə tapılmadı");
            }

            permission.IsActive = !permission.IsActive;
            permission.ModifiedDate = DateTime.Now;
            permission.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "İcazə statusu dəyişdi: {PermissionName} - Yeni status: {IsActive}",
                permission.Name, permission.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İcazə statusu dəyişərkən xəta: ID {PermissionId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>İcazəni silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeletePermissionAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .Include(p => p.UserPermissions)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (permission == null)
            {
                return (false, "İcazə tapılmadı");
            }

            // İstifadədə olan icazəni silmə
            var activeRoleCount = permission.RolePermissions.Count(rp => !rp.IsDeleted);
            var activeUserCount = permission.UserPermissions.Count(up => !up.IsDeleted);

            if (activeRoleCount > 0 || activeUserCount > 0)
            {
                return (false, $"Bu icazə istifadədədir ({activeRoleCount} rol, {activeUserCount} istifadəçi)");
            }

            permission.IsDeleted = true;
            permission.IsActive = false;
            permission.ModifiedDate = DateTime.Now;
            permission.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "İcazə silindi: {PermissionName} (ID: {PermissionId})",
                permission.Name, permission.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İcazə silinərkən xəta: ID {PermissionId}", id);
            return (false, "İcazə silinərkən xəta baş verdi");
        }
    }

    /// <summary>Kod unikallığını yoxlayır</summary>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var normalizedCode = code.ToUpperInvariant().Trim();

        var query = _context.Permissions
            .Where(p => !p.IsDeleted && p.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>HTTP metodlarını gətirir</summary>
    public List<string> GetHttpMethods()
    {
        return new List<string> { "GET", "POST", "PUT", "DELETE", "PATCH" };
    }

    /// <summary>İcazə seçim siyahısı (dropdown)</summary>
    public async Task<List<SelectListItem>> GetPermissionSelectListAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = !string.IsNullOrEmpty(p.Code)
                    ? $"{p.Name} ({p.Code})"
                    : p.Name
            })
            .ToListAsync();
    }
}