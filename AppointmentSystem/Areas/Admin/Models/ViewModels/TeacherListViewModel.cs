namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Müəllim siyahısı üçün ViewModel
/// </summary>
public class TeacherListViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ImagePath { get; set; }
    public string? Specialization { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SubjectCount { get; set; }
    public int ClassCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}