using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Fənn idarəetmə servisi
/// </summary>
public interface ISubjectService
{
    #region Query Methods

    /// <summary>Bütün fənləri gətirir</summary>
    Task<List<SubjectListViewModel>> GetAllSubjectsAsync();

    /// <summary>Aktiv fənləri gətirir</summary>
    Task<List<SubjectListViewModel>> GetActiveSubjectsAsync();

    /// <summary>ID-yə görə fənn gətirir</summary>
    Task<SubjectViewModel?> GetSubjectByIdAsync(Guid id);

    /// <summary>ID-yə görə fənn detaylarını gətirir</summary>
    Task<SubjectDetailsViewModel?> GetSubjectDetailsByIdAsync(Guid id);

    /// <summary>Koda görə fənn gətirir</summary>
    Task<Subject?> GetSubjectByCodeAsync(string code);

    /// <summary>Fənnin mövcudluğunu yoxlayır</summary>
    Task<bool> SubjectExistsAsync(Guid id);

    /// <summary>Fənn select list gətirir (dropdown üçün)</summary>
    Task<List<SelectListItem>> GetSubjectSelectListAsync();

    #endregion

    #region Command Methods

    /// <summary>Yeni fənn yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? SubjectId)> CreateSubjectAsync(
        SubjectViewModel model,
        Guid currentUserId);

    /// <summary>Fənni yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateSubjectAsync(
        SubjectViewModel model,
        Guid currentUserId);

    /// <summary>Fənn statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleSubjectStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Fənni silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteSubjectAsync(
        Guid id,
        Guid currentUserId);

    #endregion

    #region Validation Methods

    /// <summary>Ad unikallığını yoxlayır</summary>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);

    /// <summary>Kod unikallığını yoxlayır</summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);

    #endregion

    #region Statistics Methods

    /// <summary>Fənnə aid müəllimləri gətirir</summary>
    Task<List<TeacherListViewModel>> GetSubjectTeachersAsync(Guid subjectId);

    /// <summary>Fənnə aid şirkətləri gətirir</summary>
    Task<List<CompanyListViewModel>> GetSubjectCompaniesAsync(Guid subjectId);

    #endregion
}