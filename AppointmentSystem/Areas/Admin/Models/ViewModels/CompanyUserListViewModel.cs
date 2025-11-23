using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Areas.Admin.Models.ViewModels;

/// <summary>
/// Şirkət istifadəçisi (manager) siyahısı üçün ViewModel
/// </summary>
public class CompanyUserListViewModel
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public bool IsManager { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}

/// <summary>
/// Şirkət istifadəçisi (manager) əlavə/redaktə üçün ViewModel
/// </summary>
public class CompanyUserViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Şirkət seçilməlidir")]
    public Guid CompanyId { get; set; }

    [Required(ErrorMessage = "İstifadəçi seçilməlidir")]
    public Guid UserId { get; set; }

    public bool IsManager { get; set; } = false;

    public bool IsActive { get; set; } = true;
}