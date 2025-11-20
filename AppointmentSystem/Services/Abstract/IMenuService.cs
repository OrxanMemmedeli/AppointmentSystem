using AppointmentSystem.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Menu idarəetmə servisi
/// </summary>
public interface IMenuService
{
    /// <summary>Bütün menyuları gətirir</summary>
    Task<List<MenuListViewModel>> GetAllMenusAsync();

    /// <summary>Root səviyyə menyuları gətirir</summary>
    Task<List<MenuListViewModel>> GetRootMenusAsync();

    /// <summary>Parent ID-yə görə child menyuları gətirir</summary>
    Task<List<MenuListViewModel>> GetChildMenusAsync(Guid parentId);

    /// <summary>Iyerarxik menyu ağacı gətirir</summary>
    Task<List<MenuTreeViewModel>> GetMenuTreeAsync();

    /// <summary>ID-yə görə menyu gətirir</summary>
    Task<MenuViewModel?> GetMenuByIdAsync(Guid id);

    /// <summary>Parent menyu seçimləri gətirir</summary>
    Task<List<SelectListItem>> GetParentMenuSelectListAsync(Guid? excludeId = null);

    /// <summary>Yeni menyu yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? MenuId)> CreateMenuAsync(
        MenuViewModel model,
        Guid currentUserId);

    /// <summary>Menyunu yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(
        MenuViewModel model,
        Guid currentUserId);

    /// <summary>Menyu statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleMenuStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Menyunu silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteMenuAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Menyu sıralamasını yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateMenuOrderAsync(
        List<(Guid Id, int OrderIndex)> menuOrders,
        Guid currentUserId);

    /// <summary>Kod unikallığını yoxlayır</summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    /// <summary>İkon siyahısını gətirir</summary>
    List<string> GetAvailableIcons();
}