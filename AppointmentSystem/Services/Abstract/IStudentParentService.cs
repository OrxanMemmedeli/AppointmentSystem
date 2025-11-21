using AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Şagird-Valideyn əlaqəsi idarəetmə servisi
/// </summary>
public interface IStudentParentService
{
    /// <summary>Bütün şagird-valideyn əlaqələrini gətirir</summary>
    Task<List<StudentParentListViewModel>> GetAllStudentParentsAsync();

    /// <summary>Şagirdə görə valideynləri gətirir</summary>
    Task<List<StudentParentListViewModel>> GetParentsByStudentAsync(Guid studentId);

    /// <summary>Valideyinə görə şagirdləri gətirir</summary>
    Task<List<StudentParentListViewModel>> GetStudentsByParentAsync(Guid parentId);

    /// <summary>ID-yə görə əlaqə gətirir</summary>
    Task<StudentParentViewModel?> GetStudentParentByIdAsync(Guid id);

    /// <summary>Şagird-valideyn əlaqəsinin mövcudluğunu yoxlayır</summary>
    Task<bool> StudentParentExistsAsync(Guid id);

    /// <summary>Əlaqənin mövcudluğunu yoxlayır (duplicate check)</summary>
    Task<bool> RelationshipExistsAsync(Guid studentId, Guid parentId, Guid? excludeId = null);

    /// <summary>Yeni şagird-valideyn əlaqəsi yaradır</summary>
    Task<(bool Success, string? ErrorMessage, Guid? StudentParentId)> CreateStudentParentAsync(
        StudentParentViewModel model,
        Guid currentUserId);

    /// <summary>Şagird-valideyn əlaqəsini yeniləyir</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateStudentParentAsync(
        StudentParentViewModel model,
        Guid currentUserId);

    /// <summary>Əsas valideyni təyin edir</summary>
    Task<(bool Success, string? ErrorMessage)> SetPrimaryContactAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>Şagird-valideyn əlaqəsini silir</summary>
    Task<(bool Success, string? ErrorMessage)> DeleteStudentParentAsync(
        Guid id,
        Guid currentUserId);
}
