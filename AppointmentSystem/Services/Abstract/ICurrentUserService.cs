namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Cari istifadəçi məlumatlarını əldə etmək üçün interface
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? FullName { get; }
    Guid? CompanyId { get; }
    bool IsAuthenticated { get; }

    bool IsInRole(string role);
    IEnumerable<string> GetRoles();

    // ✅ Helper metodları
    Task<Guid?> GetTeacherIdAsync();
    Task<Guid?> GetParentIdAsync();
    //Task<Guid?> GetStudentIdAsync();
}
