using AppointmentSystem.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// İstifadəçi servisi (Minimal - CompanyController üçün)
/// </summary>
public interface IUserService
{
    /// <summary>İstifadəçi select list gətirir (dropdown üçün)</summary>
    Task<List<SelectListItem>> GetUserSelectListAsync();

    /// <summary>Aktiv istifadəçiləri gətirir</summary>
    Task<List<UserListViewModel>> GetActiveUsersAsync();
}