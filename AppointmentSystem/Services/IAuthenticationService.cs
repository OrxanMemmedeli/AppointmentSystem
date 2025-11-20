using AppointmentSystem.Models.ViewModels;
using System.Security.Claims;
using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAdminAsync(AdminLoginViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateTeacherAsync(TeacherLoginViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateParentAsync(ParentLoginViewModel model);
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user, Guid? companyId);
}
