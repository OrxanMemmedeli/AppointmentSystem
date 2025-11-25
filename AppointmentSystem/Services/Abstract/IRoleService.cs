using AppointmentSystem.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Role idarəetmə servisi
/// </summary>
public interface IRoleService
{
    #region Role CRUD

    /// <summary>Bütün rolları gətirir</summary>
    Task<List<RoleListViewModel>> GetAllRolesAsync();

    /// <summary>Aktiv rolları gətirir</summary>
    Task<List<RoleListViewModel>> GetActiveRolesAsync();

    /// <summary>ID-yə görə rol gətirir</summary>
    Task<RoleViewModel?> GetRoleByIdAsync(Guid id);

    /// <summary>Yeni rol yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? RoleId)> CreateRoleAsync(RoleViewModel model, Guid currentUserId);

    /// <summary>Rolun məlumatlarını yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateRoleAsync(RoleViewModel model, Guid currentUserId);

    /// <summary>Rolun statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleRoleStatusAsync(Guid id, Guid currentUserId);

    /// <summary>Rolu silir (soft delete)</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteRoleAsync(Guid id, Guid currentUserId);

    /// <summary>Rol kodunun unikallığını yoxlayır</summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    /// <summary>Rol seçim siyahısı (dropdown)</summary>
    Task<List<SelectListItem>> GetRoleSelectListAsync();

    #endregion

    #region Role-Permission Management

    /// <summary>Rola təyin olunmuş icazələri gətirir</summary>
    Task<List<PermissionListViewModel>> GetRolePermissionsAsync(Guid roleId);

    /// <summary>Rola icazə təyin edir</summary>
    Task<(bool Success, string? ErrorMessage)> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, Guid currentUserId);

    /// <summary>Roldan icazəni çıxarır</summary>
    Task<(bool Success, string? ErrorMessage)> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, Guid currentUserId);

    /// <summary>Rola çoxlu icazə təyin edir (toplu)</summary>
    Task<(bool Success, string? ErrorMessage)> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds, Guid currentUserId);

    #endregion

    #region Role-User Management

    /// <summary>Rola təyin olunmuş istifadəçiləri gətirir</summary>
    Task<List<UserListViewModel>> GetRoleUsersAsync(Guid roleId);

    /// <summary>Rola istifadəçi təyin edir</summary>
    Task<(bool Success, string? ErrorMessage)> AssignUserToRoleAsync(Guid roleId, Guid userId, Guid currentUserId);

    /// <summary>Roldan istifadəçini çıxarır</summary>
    Task<(bool Success, string? ErrorMessage)> RemoveUserFromRoleAsync(Guid roleId, Guid userId, Guid currentUserId);

    #endregion
}