using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// İstifadəçi servisi implementasiyası (Minimal)
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext context,
        ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>İstifadəçi select list gətirir</summary>
    public async Task<List<SelectListItem>> GetUserSelectListAsync()
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted && !u.IsLocked)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} ({u.UserName})"
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstifadəçi select list yüklənərkən xəta");
            return new List<SelectListItem>();
        }
    }

    /// <summary>Aktiv istifadəçiləri gətirir</summary>
    public async Task<List<UserListViewModel>> GetActiveUsersAsync()
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted)
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FirstName + " " + u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    IsEmailConfirmed = u.IsEmailConfirmed,
                    LastLoginDate = u.LastLoginDate
                })
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiv istifadəçilər yüklənərkən xəta");
            return new List<UserListViewModel>();
        }
    }
}