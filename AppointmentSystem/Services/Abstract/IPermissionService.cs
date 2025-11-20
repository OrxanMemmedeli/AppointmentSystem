using AppointmentSystem.Models.Enums;
using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Permission idarəetmə servisi
/// </summary>
public interface IPermissionService
{
    /// <summary>Bütün icazələri gətirir</summary>
    Task<List<PermissionListViewModel>> GetAllPermissionsAsync();

    /// <summary>Aktiv icazələri gətirir</summary>
    Task<List<PermissionListViewModel>> GetActivePermissionsAsync();

    /// <summary>Tipə görə icazələri gətirir</summary>
    Task<List<PermissionListViewModel>> GetPermissionsByTypeAsync(PermissionType type);

    /// <summary>ID-yə görə icazə gətirir</summary>
    Task<PermissionViewModel?> GetPermissionByIdAsync(Guid id);

    /// <summary>Yeni icazə yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? PermissionId)> CreatePermissionAsync(
        PermissionViewModel model,
        Guid currentUserId);

    /// <summary>İcazənin məlumatlarını yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdatePermissionAsync(
        PermissionViewModel model,
        Guid currentUserId);

    /// <summary>İcazənin statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> TogglePermissionStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>İcazəni silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeletePermissionAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Kod unikallığını yoxlayır</summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    /// <summary>HTTP metodlarını gətirir</summary>
    List<string> GetHttpMethods();
}