using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Şagird idarəetmə servisi
/// </summary>
public interface IStudentService
{
    /// <summary>Bütün şagirdləri gətirir</summary>
    Task<List<StudentListViewModel>> GetAllStudentsAsync();

    /// <summary>Aktiv şagirdləri gətirir</summary>
    Task<List<StudentListViewModel>> GetActiveStudentsAsync();

    /// <summary>Şirkətə görə şagirdləri gətirir</summary>
    Task<List<StudentListViewModel>> GetStudentsByCompanyAsync(Guid companyId);

    /// <summary>Sinfə görə şagirdləri gətirir</summary>
    Task<List<StudentListViewModel>> GetStudentsByClassAsync(Guid classId);

    /// <summary>ID-yə görə şagird gətirir</summary>
    Task<StudentViewModel?> GetStudentByIdAsync(Guid id);

    /// <summary>ID-yə görə şagird detaylarını gətirir</summary>
    Task<StudentDetailsViewModel?> GetStudentDetailsByIdAsync(Guid id);

    /// <summary>FIN koda görə şagird gətirir</summary>
    Task<Student?> GetStudentByFinCodeAsync(string finCode);

    /// <summary>Şagirdin mövcudluğunu yoxlayır</summary>
    Task<bool> StudentExistsAsync(Guid id);

    /// <summary>Yeni şagird yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? StudentId)> CreateStudentAsync(
        StudentViewModel model,
        Guid currentUserId);

    /// <summary>Şagirdi yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateStudentAsync(
        StudentViewModel model,
        Guid currentUserId);

    /// <summary>Şagird statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleStudentStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şagirdi silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteStudentAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şəkil yükləyir</summary>
    Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid studentId);

    /// <summary>Şəkli silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid studentId);

    /// <summary>FIN kod unikallığını yoxlayır</summary>
    Task<bool> IsFinCodeUniqueAsync(string finCode, Guid? excludeId = null);

    /// <summary>Şagird select list gətirir</summary>
    Task<List<SelectListItem>> GetStudentSelectListAsync(Guid? companyId = null);

    /// <summary>Şagirdin valideynlərini gətirir</summary>
    Task<List<StudentParentInfo>> GetStudentParentsAsync(Guid studentId);
}