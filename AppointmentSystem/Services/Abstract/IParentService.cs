using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Valideyn idarəetmə servisi
/// </summary>
public interface IParentService
{
    /// <summary>Bütün valideynləri gətirir</summary>
    Task<List<ParentListViewModel>> GetAllParentsAsync();

    /// <summary>Aktiv valideynləri gətirir</summary>
    Task<List<ParentListViewModel>> GetActiveParentsAsync();

    /// <summary>Şirkətə görə valideynləri gətirir</summary>
    Task<List<ParentListViewModel>> GetParentsByCompanyAsync(Guid companyId);

    /// <summary>ID-yə görə valideyn gətirir</summary>
    Task<ParentViewModel?> GetParentByIdAsync(Guid id);

    /// <summary>ID-yə görə valideyn detaylarını gətirir</summary>
    Task<ParentDetailsViewModel?> GetParentDetailsByIdAsync(Guid id);

    /// <summary>FIN koda görə valideyn gətirir</summary>
    Task<Parent?> GetParentByFinCodeAsync(string finCode);

    /// <summary>Valideyinin mövcudluğunu yoxlayır</summary>
    Task<bool> ParentExistsAsync(Guid id);

    /// <summary>Yeni valideyn yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? ParentId)> CreateParentAsync(
        ParentViewModel model,
        Guid currentUserId);

    /// <summary>Valideyni yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateParentAsync(
        ParentViewModel model,
        Guid currentUserId);

    /// <summary>Valideyn statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleParentStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Valideyni silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteParentAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şəkil yükləyir</summary>
    Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid parentId);

    /// <summary>Şəkli silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid parentId);

    /// <summary>FIN kod unikallığını yoxlayır</summary>
    Task<bool> IsFinCodeUniqueAsync(string finCode, Guid? excludeId = null);

    /// <summary>Valideyn select list gətirir</summary>
    Task<List<SelectListItem>> GetParentSelectListAsync(Guid? companyId = null);

    /// <summary>Valideyinin uşaqlarını gətirir</summary>
    Task<List<ParentChildInfo>> GetParentChildrenAsync(Guid parentId);
}