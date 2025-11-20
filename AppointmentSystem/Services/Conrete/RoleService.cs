using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Conrete;

/// <summary>
/// Role idarəetmə servisi implementasiyası
/// </summary>
public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        AppDbContext context,
        ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün rolları gətirir (statistika ilə)</summary>
    public async Task<List<RoleListViewModel>> GetAllRolesAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .Select(r => new RoleListViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Description = r.Description,
                Priority = r.Priority,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                UserCount = r.UserRoles.Count(ur => !ur.IsDeleted && ur.User.IsActive),
                PermissionCount = r.RolePermissions.Count(rp => !rp.IsDeleted),
                CreatedDate = r.CreatedDate
            })
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    /// <summary>Aktiv rolları gətirir</summary>
    public async Task<List<RoleListViewModel>> GetActiveRolesAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.IsActive)
            .Select(r => new RoleListViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Description = r.Description,
                Priority = r.Priority,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                CreatedDate = r.CreatedDate
            })
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    /// <summary>ID-yə görə rol gətirir</summary>
    public async Task<RoleViewModel?> GetRoleByIdAsync(Guid id)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Description = r.Description,
                Priority = r.Priority,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive
            })
            .FirstOrDefaultAsync();

        return role;
    }

    /// <summary>Yeni rol yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? RoleId)> CreateRoleAsync(
        RoleViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Kod unikallığını yoxla
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var codeExists = await IsCodeUniqueAsync(model.Code);
                if (!codeExists)
                {
                    return (false, "Bu kod artıq istifadə olunur", null);
                }
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Code = model.Code?.ToUpperInvariant().Trim(),
                Description = model.Description?.Trim(),
                Priority = model.Priority,
                IsSystemRole = model.IsSystemRole,
                IsActive = model.IsActive,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedById = currentUserId
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni rol yaradıldı: {RoleName} (ID: {RoleId}) - Yaradan: {UserId}",
                role.Name, role.Id, currentUserId);

            return (true, null, role.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rol yaradılarkən xəta: {RoleName}", model.Name);
            return (false, "Rol yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Rolun məlumatlarını yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateRoleAsync(
        RoleViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Rol ID-si tələb olunur");
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == model.Id.Value && !r.IsDeleted);

            if (role == null)
            {
                return (false, "Rol tapılmadı");
            }

            // Sistem rolunun kod və sistem statusunu dəyişməyə icazə vermə
            if (role.IsSystemRole && (!model.IsSystemRole || role.Code != model.Code))
            {
                return (false, "Sistem rolunun əsas parametrləri dəyişdirilə bilməz");
            }

            // Kod unikallığını yoxla
            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != role.Code)
            {
                var codeExists = await IsCodeUniqueAsync(model.Code, role.Id);
                if (!codeExists)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            role.Name = model.Name.Trim();
            role.Code = model.Code?.ToUpperInvariant().Trim();
            role.Description = model.Description?.Trim();
            role.Priority = model.Priority;
            role.IsActive = model.IsActive;
            role.ModifiedDate = DateTimeOffset.UtcNow;
            role.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Rol yeniləndi: {RoleName} (ID: {RoleId}) - Yeniləyən: {UserId}",
                role.Name, role.Id, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rol yenilənərkən xəta: ID {RoleId}", model.Id);
            return (false, "Rol yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Rolun statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleRoleStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (role == null)
            {
                return (false, "Rol tapılmadı");
            }

            if (role.IsSystemRole)
            {
                return (false, "Sistem rolunun statusu dəyişdirilə bilməz");
            }

            role.IsActive = !role.IsActive;
            role.ModifiedDate = DateTimeOffset.UtcNow;
            role.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Rolun statusu dəyişdi: {RoleName} - Yeni status: {IsActive}",
                role.Name, role.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rol statusu dəyişərkən xəta: ID {RoleId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Rolu silir (soft delete)</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteRoleAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (role == null)
            {
                return (false, "Rol tapılmadı");
            }

            if (role.IsSystemRole)
            {
                return (false, "Sistem rolu silinə bilməz");
            }

            // İstifadədə olan rolu silmə
            var activeUserCount = role.UserRoles.Count(ur => !ur.IsDeleted);
            if (activeUserCount > 0)
            {
                return (false, $"Bu rol {activeUserCount} istifadəçiyə təyin olunub. Əvvəlcə rol təyinlərini silin.");
            }

            role.IsDeleted = true;
            role.IsActive = false;
            role.ModifiedDate = DateTimeOffset.UtcNow;
            role.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Rol silindi: {RoleName} (ID: {RoleId}) - Silən: {UserId}",
                role.Name, role.Id, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rol silinərkən xəta: ID {RoleId}", id);
            return (false, "Rol silinərkən xəta baş verdi");
        }
    }

    /// <summary>Rol kodunun unikallığını yoxlayır</summary>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var normalizedCode = code.ToUpperInvariant().Trim();

        var query = _context.Roles
            .Where(r => !r.IsDeleted && r.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}