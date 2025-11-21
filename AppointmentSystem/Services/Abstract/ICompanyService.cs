using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Şirkət idarəetmə servisi
/// </summary>
public interface ICompanyService
{
    /// <summary>Bütün şirkətləri gətirir</summary>
    Task<List<CompanyListViewModel>> GetAllCompaniesAsync();

    /// <summary>Aktiv şirkətləri gətirir</summary>
    Task<List<CompanyListViewModel>> GetActiveCompaniesAsync();

    /// <summary>Bütün aktiv şirkətləri gətirir (alias metod)</summary>
    Task<List<CompanyListViewModel>> GetAllActiveCompaniesAsync();

    /// <summary>ID-yə görə şirkət gətirir</summary>
    Task<CompanyViewModel?> GetCompanyByIdAsync(Guid id);

    /// <summary>ID-yə görə şirkət entity-si gətirir</summary>
    Task<Company?> GetCompanyEntityByIdAsync(Guid id);

    /// <summary>Koda görə şirkət gətirir</summary>
    Task<Company?> GetCompanyByCodeAsync(string code);

    /// <summary>Şirkətin mövcudluğunu yoxlayır</summary>
    Task<bool> CompanyExistsAsync(Guid id);

    /// <summary>Yeni şirkət yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? CompanyId)> CreateCompanyAsync(
        CompanyViewModel model,
        Guid currentUserId);

    /// <summary>Şirkəti yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateCompanyAsync(
        CompanyViewModel model,
        Guid currentUserId);

    /// <summary>Şirkət statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleCompanyStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şirkəti doğrulayır (admin üçün)</summary>
    Task<(bool Success, string? ErrorMessage)> VerifyCompanyAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şirkəti silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteCompanyAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Logo yükləyir</summary>
    Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadLogoAsync(
        IFormFile file,
        Guid companyId);

    /// <summary>Background şəkil yükləyir</summary>
    Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadBackgroundImageAsync(
        IFormFile file,
        Guid companyId);

    /// <summary>Logonu silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteLogoAsync(Guid companyId);

    /// <summary>Background şəkli silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteBackgroundImageAsync(Guid companyId);

    /// <summary>Kod unikallığını yoxlayır</summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    /// <summary>Şirkət select list gətirir</summary>
    Task<List<SelectListItem>> GetCompanySelectListAsync();

    /// <summary>
    /// Aktiv şirkətlərin kart məlumatlarını gətirir (public seçim səhifəsi üçün)
    /// </summary>
    Task<List<CompanyCardViewModel>> GetCompanyCardsAsync();
}