using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Müəllim idarəetmə servisi
/// </summary>
public interface ITeacherService
{
    #region Query Methods

    /// <summary>Bütün müəllimləri gətirir</summary>
    Task<List<TeacherListViewModel>> GetAllTeachersAsync();

    /// <summary>Aktiv müəllimləri gətirir</summary>
    Task<List<TeacherListViewModel>> GetActiveTeachersAsync();

    /// <summary>Şirkətə görə müəllimləri gətirir</summary>
    Task<List<TeacherListViewModel>> GetTeachersByCompanyAsync(Guid companyId);

    /// <summary>ID-yə görə müəllim gətirir</summary>
    Task<TeacherViewModel?> GetTeacherByIdAsync(Guid id);

    /// <summary>ID-yə görə müəllim detaylarını gətirir</summary>
    Task<TeacherDetailsViewModel?> GetTeacherDetailsByIdAsync(Guid id);

    /// <summary>User ID-yə görə müəllim gətirir</summary>
    Task<Teacher?> GetTeacherByUserIdAsync(Guid userId);

    /// <summary>Email-ə görə müəllim gətirir</summary>
    Task<Teacher?> GetTeacherByEmailAsync(string email);

    /// <summary>Müəllimin mövcudluğunu yoxlayır</summary>
    Task<bool> TeacherExistsAsync(Guid id);

    /// <summary>Müəllim select list gətirir (dropdown üçün)</summary>
    Task<List<SelectListItem>> GetTeacherSelectListAsync(Guid? companyId = null);

    #endregion

    #region Command Methods

    /// <summary>Yeni müəllim yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? TeacherId)> CreateTeacherAsync(
        TeacherViewModel model,
        Guid currentUserId);

    /// <summary>Müəllimi yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateTeacherAsync(
        TeacherViewModel model,
        Guid currentUserId);

    /// <summary>Müəllim statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleTeacherStatusAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Müəllimi silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteTeacherAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şəkil yükləyir</summary>
    Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid teacherId);

    /// <summary>Şəkli silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid teacherId);

    #endregion

    #region Validation Methods

    /// <summary>Email unikallığını yoxlayır</summary>
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);

    /// <summary>User ID-nin istifadə olunub-olunmadığını yoxlayır</summary>
    Task<bool> IsUserIdAvailableAsync(Guid userId, Guid? excludeId = null);

    #endregion

    #region TeacherSubject Management (Junction Table)

    /// <summary>Müəllimin fənlərini gətirir</summary>
    Task<List<SubjectListViewModel>> GetTeacherSubjectsAsync(Guid teacherId);

    /// <summary>Müəllimə fənn əlavə edir</summary>
    Task<(bool Success, string? ErrorMessage)> AssignSubjectToTeacherAsync(
        Guid teacherId,
        Guid subjectId,
        Guid currentUserId);

    /// <summary>Müəllimdən fənni çıxarır</summary>
    Task<(bool Success, string? ErrorMessage)> RemoveSubjectFromTeacherAsync(
        Guid teacherId,
        Guid subjectId,
        Guid currentUserId);

    /// <summary>Fənnin müəllimə təyin olunub-olunmadığını yoxlayır</summary>
    Task<bool> IsSubjectAssignedToTeacherAsync(Guid teacherId, Guid subjectId);

    #endregion

    #region TeacherClass Management (Junction Table)

    /// <summary>Müəllimin siniflərini gətirir</summary>
    Task<List<TeacherClassViewModel>> GetTeacherClassesAsync(Guid teacherId);

    /// <summary>Müəllimə sinif əlavə edir</summary>
    Task<(bool Success, string? ErrorMessage)> AssignClassToTeacherAsync(
        Guid teacherId,
        Guid classId,
        Guid? subjectId,
        bool isClassLeader,
        Guid currentUserId);

    /// <summary>Müəllimdən sinfi çıxarır</summary>
    Task<(bool Success, string? ErrorMessage)> RemoveClassFromTeacherAsync(
        Guid teacherId,
        Guid classId,
        Guid currentUserId);

    /// <summary>Sinif rəhbəri statusunu dəyişir</summary>
    Task<(bool Success, string? ErrorMessage)> ToggleClassLeaderAsync(
        Guid teacherId,
        Guid classId,
        Guid currentUserId);

    /// <summary>Sinfin müəllimə təyin olunub-olunmadığını yoxlayır</summary>
    Task<bool> IsClassAssignedToTeacherAsync(Guid teacherId, Guid classId);

    #endregion

    #region Statistics Methods

    /// <summary>Müəllimin görüşlərini gətirir</summary>
    Task<List<MeetingListViewModel>> GetTeacherMeetingsAsync(Guid teacherId);

    #endregion
}