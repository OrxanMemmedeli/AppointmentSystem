using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Role idarəetmə servisi
/// </summary>
public interface IRoleService
{
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
}
