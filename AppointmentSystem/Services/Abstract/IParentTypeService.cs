using AppointmentSystem.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Valideyn növü idarəetmə servisi
/// </summary>
public interface IParentTypeService
{
    /// <summary>Bütün valideyn növlərini gətirir</summary>
    Task<List<ParentTypeListViewModel>> GetAllParentTypesAsync();

    /// <summary>Aktiv valideyn növlərini gətirir</summary>
    Task<List<ParentTypeListViewModel>> GetActiveParentTypesAsync();

    /// <summary>ID-yə görə valideyn növü gətirir</summary>
    Task<ParentTypeViewModel?> GetParentTypeByIdAsync(Guid id);

    /// <summary>Valideyn növünün mövcudluğunu yoxlayır</summary>
    Task<bool> ParentTypeExistsAsync(Guid id);

    /// <summary>Yeni valideyn növü yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? ParentTypeId)> CreateParentTypeAsync(
        ParentTypeViewModel model,
        Guid currentUserId);

    /// <summary>Valideyn növünü yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateParentTypeAsync(
        ParentTypeViewModel model,
        Guid currentUserId);

    /// <summary>Valideyn növü statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleParentTypeStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Valideyn növünü silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteParentTypeAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Ad unikallığını yoxlayır</summary>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);

    /// <summary>Valideyn növü select list gətirir</summary>
    Task<List<SelectListItem>> GetParentTypeSelectListAsync();
}