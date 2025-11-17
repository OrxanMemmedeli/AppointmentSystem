using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateParentAsync(ParentLoginViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateTeacherAsync(TeacherLoginViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAdminAsync(AdminLoginViewModel model);
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user, Guid companyId);
}

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateParentAsync(ParentLoginViewModel model)
    {
        // Valideyn-i FIN və Ad/Soyad əsasında tap
        var parent = await _context.Parents
            .Include(p => p.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(p => p.CompanyId == model.CompanyId &&
                       p.FinCode == model.FinCode.ToUpper() &&
                       p.FirstName.ToLower() == model.FirstName.ToLower() &&
                       p.LastName.ToLower() == model.LastName.ToLower())
            .FirstOrDefaultAsync();

        if (parent == null)
            return (false, "Valideyn tapılmadı. Məlumatları yoxlayın.", null);

        // Əgər User yoxdursa, yarat
        if (parent.User == null)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = parent.FirstName,
                LastName = parent.LastName,
                Email = parent.Email ?? $"{parent.FinCode}@parent.local",
                UserName = $"P_{parent.FinCode}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(parent.FinCode), // FIN kod şifrə kimi
                UserTypeId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Parent Type
                IsEmailConfirmed = true,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.UtcNow
            };

            _context.Users.Add(user);

            // Parent role əlavə et
            var parentRoleId = Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD");
            _context.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = parentRoleId,
                AssignedDate = DateTimeOffset.UtcNow,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.UtcNow
            });

            parent.UserId = user.Id;
            await _context.SaveChangesAsync();

            parent.User = user;
        }

        if (parent.User.IsLocked)
            return (false, "Hesabınız bloklanıb. Administrator ilə əlaqə saxlayın.", null);

        // Son giriş tarixini yenilə
        parent.User.LastLoginDate = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, parent.User);
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateTeacherAsync(TeacherLoginViewModel model)
    {
        var teacher = await _context.Teachers
            .Include(t => t.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(t => t.CompanyId == model.CompanyId &&
                       t.Email.ToLower() == model.Email.ToLower())
            .FirstOrDefaultAsync();

        if (teacher == null || teacher.User == null)
            return (false, "Email və ya şifrə yanlışdır.", null);

        if (teacher.User.IsLocked)
            return (false, "Hesabınız bloklanıb. Administrator ilə əlaqə saxlayın.", null);

        // Şifrəni yoxla
        if (!BCrypt.Net.BCrypt.Verify(model.Password, teacher.User.PasswordHash))
        {
            // Uğursuz cəhdləri artır - DÜZƏLDILDI
            teacher.User.FailedLoginAttempts += 1;

            if (teacher.User.FailedLoginAttempts >= 5)
            {
                teacher.User.IsLocked = true;
                teacher.User.LockoutEnd = DateTimeOffset.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();
                return (false, "Çox sayda uğursuz cəhd. Hesabınız 1 saat bloklanıb.", null);
            }

            await _context.SaveChangesAsync();
            return (false, "Email və ya şifrə yanlışdır.", null);
        }

        // Uğurlu giriş
        teacher.User.FailedLoginAttempts = 0;
        teacher.User.LastLoginDate = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, teacher.User);
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAdminAsync(AdminLoginViewModel model)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => (u.UserName.ToLower() == model.UserNameOrEmail.ToLower() ||
                        u.Email.ToLower() == model.UserNameOrEmail.ToLower()))
            .FirstOrDefaultAsync();

        if (user == null)
            return (false, "İstifadəçi adı və ya şifrə yanlışdır.", null);

        if (user.IsLocked)
            return (false, "Hesabınız bloklanıb. Administrator ilə əlaqə saxlayın.", null);

        // Şifrəni yoxla
        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            // DÜZƏLDILDI
            user.FailedLoginAttempts += 1;

            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();
                return (false, "Çox sayda uğursuz cəhd. Hesabınız 1 saat bloklanıb.", null);
            }

            await _context.SaveChangesAsync();
            return (false, "İstifadəçi adı və ya şifrə yanlışdır.", null);
        }

        // Admin və ya Super Admin rolunu yoxla
        var hasAdminRole = user.UserRoles.Any(ur =>
            ur.Role.Code == "SUPER_ADMIN" || ur.Role.Code == "COMPANY_ADMIN");

        if (!hasAdminRole)
            return (false, "Bu girişə icazəniz yoxdur.", null);

        // Uğurlu giriş
        user.FailedLoginAttempts = 0;
        user.LastLoginDate = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, user);
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user, Guid companyId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
            new("CompanyId", companyId.ToString())
        };

        // Rolları əlavə et
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Code!)
            .ToListAsync();

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "AppointmentSystemAuth");
        return new ClaimsPrincipal(identity);
    }
}