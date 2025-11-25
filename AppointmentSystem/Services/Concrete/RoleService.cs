using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Concrete;

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
                CreatedDate = DateTime.Now,
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
            role.ModifiedDate = DateTime.Now;
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
            role.ModifiedDate = DateTime.Now;
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
            role.ModifiedDate = DateTime.Now;
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

    /// <summary>Rol seçim siyahısı (dropdown)</summary>
    public async Task<List<SelectListItem>> GetRoleSelectListAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            })
            .ToListAsync();
    }

    #region Role-Permission Management

    /// <summary>Rola təyin olunmuş icazələri gətirir</summary>
    public async Task<List<PermissionListViewModel>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => new PermissionListViewModel
            {
                Id = rp.PermissionId,
                Name = rp.Permission.Name,
                Code = rp.Permission.Code,
                Description = rp.Permission.Description,
                Type = rp.Permission.Type,
                AreaName = rp.Permission.AreaName,
                ControllerName = rp.Permission.ControllerName,
                ActionName = rp.Permission.ActionName,
                IsActive = rp.Permission.IsActive,
                CreatedDate = rp.Permission.CreatedDate
            })
            .OrderBy(p => p.AreaName)
            .ThenBy(p => p.ControllerName)
            .ThenBy(p => p.ActionName)
            .ToListAsync();
    }

    /// <summary>Rola icazə təyin edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignPermissionToRoleAsync(
        Guid roleId, Guid permissionId, Guid currentUserId)
    {
        try
        {
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

            if (exists)
            {
                return (false, "Bu icazə artıq rola təyin edilib");
            }

            var rolePermission = new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rola icazə təyin edildi: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rola icazə təyin edilərkən xəta: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
            return (false, "İcazə təyin edilərkən xəta baş verdi");
        }
    }

    /// <summary>Roldan icazəni çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemovePermissionFromRoleAsync(
        Guid roleId, Guid permissionId, Guid currentUserId)
    {
        try
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

            if (rolePermission == null)
            {
                return (false, "Bu icazə rolda tapılmadı");
            }

            rolePermission.IsDeleted = true;
            rolePermission.ModifiedDate = DateTime.Now;
            rolePermission.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Roldan icazə çıxarıldı: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Roldan icazə çıxarılarkən xəta: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
            return (false, "İcazə çıxarılarkən xəta baş verdi");
        }
    }

    /// <summary>Rola çoxlu icazə təyin edir (toplu)</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignPermissionsToRoleAsync(
        Guid roleId, List<Guid> permissionIds, Guid currentUserId)
    {
        try
        {
            var existingAssignments = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
                .ToListAsync();

            foreach (var assignment in existingAssignments)
            {
                assignment.IsDeleted = true;
                assignment.ModifiedDate = DateTime.Now;
                assignment.ModifiedById = currentUserId;
            }

            foreach (var permissionId in permissionIds)
            {
                var rolePermission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedById = currentUserId
                };
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rola toplu icazələr təyin edildi: RoleId={RoleId}, Count={Count}", roleId, permissionIds.Count);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rola toplu icazə təyin edilərkən xəta: RoleId={RoleId}", roleId);
            return (false, "İcazələr təyin edilərkən xəta baş verdi");
        }
    }

    #endregion

    #region Role-User Management

    /// <summary>Rola təyin olunmuş istifadəçiləri gətirir</summary>
    public async Task<List<UserListViewModel>> GetRoleUsersAsync(Guid roleId)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId && !ur.IsDeleted)
            .Select(ur => new UserListViewModel
            {
                Id = ur.UserId,
                UserName = ur.User.UserName,
                Email = ur.User.Email,
                FirstName = ur.User.FirstName,
                LastName = ur.User.LastName,
                FullName = ur.User.FirstName + " " + ur.User.LastName,
                IsActive = ur.User.IsActive,
                CreatedDate = ur.User.CreatedDate
            })
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    /// <summary>Rola istifadəçi təyin edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignUserToRoleAsync(
        Guid roleId, Guid userId, Guid currentUserId)
    {
        try
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.RoleId == roleId && ur.UserId == userId && !ur.IsDeleted);

            if (exists)
            {
                return (false, "Bu istifadəçi artıq rola təyin edilib");
            }

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                UserId = userId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rola istifadəçi təyin edildi: RoleId={RoleId}, UserId={UserId}", roleId, userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rola istifadəçi təyin edilərkən xəta: RoleId={RoleId}, UserId={UserId}", roleId, userId);
            return (false, "İstifadəçi təyin edilərkən xəta baş verdi");
        }
    }

    /// <summary>Roldan istifadəçini çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveUserFromRoleAsync(
        Guid roleId, Guid userId, Guid currentUserId)
    {
        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.RoleId == roleId && ur.UserId == userId && !ur.IsDeleted);

            if (userRole == null)
            {
                return (false, "Bu istifadəçi rolda tapılmadı");
            }

            userRole.IsDeleted = true;
            userRole.ModifiedDate = DateTime.Now;
            userRole.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Roldan istifadəçi çıxarıldı: RoleId={RoleId}, UserId={UserId}", roleId, userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Roldan istifadəçi çıxarılarkən xəta: RoleId={RoleId}, UserId={UserId}", roleId, userId);
            return (false, "İstifadəçi çıxarılarkən xəta baş verdi");
        }
    }

    #endregion
}