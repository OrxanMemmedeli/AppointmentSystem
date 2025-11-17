namespace AppointmentSystem.Services;

/// <summary>
/// Cari istifadəçi məlumatlarını əldə etmək üçün interface
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IEnumerable<string> GetRoles();
}
