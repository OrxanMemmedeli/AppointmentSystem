using AppointmentSystem.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Sinif idarəetmə servisi
/// </summary>
public interface ISchoolClassService
{
    /// <summary>Bütün sinifləri gətirir</summary>
    Task<List<SchoolClassListViewModel>> GetAllClassesAsync();

    /// <summary>Aktiv sinifləri gətirir</summary>
    Task<List<SchoolClassListViewModel>> GetActiveClassesAsync();

    /// <summary>Şirkətə görə sinifləri gətirir</summary>
    Task<List<SchoolClassListViewModel>> GetClassesByCompanyAsync(Guid companyId);

    /// <summary>ID-yə görə sinif gətirir</summary>
    Task<SchoolClassViewModel?> GetClassByIdAsync(Guid id);

    /// <summary>ID-yə görə sinif detaylarını gətirir</summary>
    Task<SchoolClassDetailsViewModel?> GetClassDetailsByIdAsync(Guid id);

    /// <summary>Sinfin mövcudluğunu yoxlayır</summary>
    Task<bool> ClassExistsAsync(Guid id);

    /// <summary>Yeni sinif yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? ClassId)> CreateClassAsync(
        SchoolClassViewModel model,
        Guid currentUserId);

    /// <summary>Sinfi yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateClassAsync(
        SchoolClassViewModel model,
        Guid currentUserId);

    /// <summary>Sinif statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleClassStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Sinfi silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteClassAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Sinif adının unikallığını yoxlayır (şirkət daxilində)</summary>
    Task<bool> IsClassNameUniqueAsync(string name, Guid companyId, Guid? excludeId = null);

    /// <summary>Sinif select list gətirir</summary>
    Task<List<SelectListItem>> GetClassSelectListAsync(Guid? companyId = null);

    /// <summary>Səviyyəyə görə sinifləri gətirir</summary>
    Task<List<SchoolClassListViewModel>> GetClassesByLevelAsync(int level, Guid? companyId = null);

    /// <summary>Sinif select list gətirir</summary>
    Task<List<SelectListItem>> GetSchoolClassSelectListAsync(Guid? companyId = null);

}