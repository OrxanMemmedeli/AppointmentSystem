using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Models.ViewModels;
using AdminViewModels = AppointmentSystem.Areas.Admin.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Meeting idarəetmə servisi - Unified interface
/// Admin, Teacher, Parent area-ları üçün ümumi servis
/// </summary>
public interface IMeetingService
{
    #region COMMON/SHARED METHODS

    /// <summary>
    /// Görüş detallarını gətirir (bütün area-lar üçün - Meeting entity)
    /// </summary>
    Task<Meeting?> GetMeetingDetailsAsync(Guid meetingId);

    /// <summary>
    /// Müəllimin müsait vaxt slotlarını gətirir (Parent üçün)
    /// </summary>
    Task<List<TimeSpan>> GetAvailableTimeSlotsAsync(Guid teacherId, DateTime date);

    /// <summary>
    /// Müəllimin müsait olub-olmadığını yoxlayır
    /// </summary>
    Task<bool> IsTeacherAvailableAsync(
        Guid teacherId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeMeetingId = null);

    /// <summary>
    /// Valideynin müsait olub-olmadığını yoxlayır
    /// </summary>
    Task<bool> IsParentAvailableAsync(
        Guid parentId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeMeetingId = null);

    #endregion

    #region ADMIN AREA METHODS

    /// <summary>
    /// [ADMIN] Filtrasiya və pagination ilə görüşlərin siyahısını gətirir
    /// </summary>
    Task<AdminViewModels.PaginatedMeetingListViewModel> GetMeetingsAsync(
        AdminViewModels.MeetingFilterViewModel filter,
        Guid? companyId = null);

    /// <summary>
    /// [ADMIN] ID-yə görə görüş məlumatlarını gətirir (Admin Details view üçün)
    /// </summary>
    Task<AdminViewModels.MeetingDetailsViewModel?> GetMeetingByIdAsync(Guid id);

    /// <summary>
    /// [ADMIN] Edit üçün görüş məlumatlarını gətirir
    /// </summary>
    Task<AdminViewModels.MeetingViewModel?> GetMeetingForEditAsync(Guid id);

    /// <summary>
    /// [ADMIN] Yeni görüş yaradır (Admin manual create)
    /// </summary>
    Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(
        AdminViewModels.MeetingViewModel model,
        Guid currentUserId);

    /// <summary>
    /// [ADMIN] Görüşü yeniləyir
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> UpdateMeetingAsync(
        AdminViewModels.MeetingViewModel model,
        Guid currentUserId);

    /// <summary>
    /// [ADMIN] Görüşü silir (soft delete)
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeleteMeetingAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>
    /// [ADMIN] Görüşü tamamlanmış kimi işarələyir
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> CompleteMeetingAsync(
        Guid id,
        Guid currentUserId);

    /// <summary>
    /// [ADMIN/DASHBOARD] Bu günkü görüşlərin sayını qaytarır
    /// </summary>
    Task<int> GetTodayMeetingsCountAsync(Guid? companyId = null);

    /// <summary>
    /// [ADMIN/DASHBOARD] Gözləyən görüşlərin sayını qaytarır
    /// </summary>
    Task<int> GetPendingMeetingsCountAsync(Guid? companyId = null);

    #endregion

    #region TEACHER AREA METHODS

    /// <summary>
    /// [TEACHER] Müəllimin görüşlərini gətirir (calendar üçün)
    /// </summary>
    Task<List<TeacherMeetingViewModel>> GetTeacherMeetingsAsync(
        Guid teacherId,
        DateTime? date = null);

    /// <summary>
    /// [TEACHER] Müəllimin görüşlərini gətirir (Meeting entity ilə)
    /// </summary>
    Task<List<Meeting>> GetMeetingsByTeacherAsync(
        Guid teacherId,
        DateTime? date = null,
        MeetingStatus? status = null);

    /// <summary>
    /// [TEACHER] Görüşü təsdiqləyir
    /// </summary>
    Task<bool> ApproveMeetingAsync(
        Guid meetingId,
        Guid approvedById,
        string? teacherNotes);

    /// <summary>
    /// [TEACHER] Görüşdən imtina edir
    /// </summary>
    Task<bool> DeclineMeetingAsync(
        Guid meetingId,
        Guid declinedById,
        string declineReason,
        string? teacherNotes);

    #endregion

    #region PARENT AREA METHODS

    /// <summary>
    /// [PARENT] Valideyn görüş yaradır (request)
    /// </summary>
    Task<(bool Success, string? ErrorMessage, Guid? MeetingId)> CreateMeetingAsync(
        Guid parentId,
        CreateMeetingViewModel model);

    /// <summary>
    /// [PARENT] Valideynin görüşlərini gətirir (ViewModel ilə)
    /// </summary>
    Task<List<ParentMeetingViewModel>> GetParentMeetingsAsync(
        Guid parentId,
        Guid companyId);

    /// <summary>
    /// [PARENT] Valideynin görüşlərini gətirir (Meeting entity ilə)
    /// </summary>
    Task<List<Meeting>> GetMeetingsByParentAsync(
        Guid parentId,
        MeetingStatus? status = null);

    /// <summary>
    /// [PARENT] Görüşü ləğv edir
    /// </summary>
    Task<bool> CancelMeetingAsync(
        Guid meetingId,
        string? cancellationReason);

    #endregion
}