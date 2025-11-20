using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentSystem.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(AppDbContext context, ILogger<AuthenticationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Admin Authentication

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAdminAsync(AdminLoginViewModel model)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    (u.Email == model.UserNameOrEmail || u.UserName == model.UserNameOrEmail) &&
                    u.IsActive && !u.IsDeleted);

            if (user == null)
                return (false, "İstifadəçi adı və ya şifrə yanlışdır", null);

            // ✅ Lockout yoxlaması
            var lockoutCheck = CheckLockout(user);
            if (!lockoutCheck.Success)
                return lockoutCheck;

            // ✅ Şifrə yoxlaması
            var passwordCheck = await ValidatePasswordAsync(user, model.Password);
            if (!passwordCheck.Success)
                return passwordCheck;

            // Admin rolunu yoxla
            var isAdmin = user.UserRoles.Any(ur =>
                ur.Role.IsActive &&
                (ur.Role.Code == "SUPERADMIN" || ur.Role.Code == "MANAGER"));

            if (!isAdmin)
                return (false, "Bu istifadəçinin admin girişi yoxdur", null);

            // ✅ Uğurlu giriş
            await UpdateSuccessfulLoginAsync(user);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin authentication xətası");
            return (false, "Sistemdə xəta baş verdi", null);
        }
    }

    #endregion

    #region Teacher Authentication

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateTeacherAsync(TeacherLoginViewModel model)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(t =>
                    t.CompanyId == model.CompanyId &&
                    t.User.Email == model.Email &&
                    t.IsActive &&
                    t.User.IsActive &&
                    !t.User.IsDeleted);

            if (teacher == null)
                return (false, "Email və ya şifrə yanlışdır", null);

            var user = teacher.User;

            // ✅ Lockout yoxlaması
            var lockoutCheck = CheckLockout(user);
            if (!lockoutCheck.Success)
                return lockoutCheck;

            // ✅ Şifrə yoxlaması
            var passwordCheck = await ValidatePasswordAsync(user, model.Password);
            if (!passwordCheck.Success)
                return passwordCheck;

            // Teacher rolunu yoxla
            var hasTeacherRole = user.UserRoles.Any(ur =>
                ur.Role.IsActive && ur.Role.Code == "TEACHER");

            if (!hasTeacherRole)
                return (false, "Bu istifadəçinin müəllim girişi yoxdur", null);

            // ✅ Uğurlu giriş
            await UpdateSuccessfulLoginAsync(user);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teacher authentication xətası");
            return (false, "Sistemdə xəta baş verdi", null);
        }
    }

    #endregion

    #region Parent Authentication

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateParentAsync(ParentLoginViewModel model)
    {
        try
        {
            // FIN və Initials-i normalize et
            var normalizedFin = model.FinCode.ToUpperInvariant().Trim();
            var normalizedInitials = (model.Initials ?? string.Empty).ToUpperInvariant().Trim();

            if (normalizedFin.Length == 0 || normalizedInitials.Length != 2)
                return (false, "FIN və ya baş hərflər düzgün deyil", null);

            // İlk və son baş hərfləri ayır (stringdə artıq trim edilmişdir)
            var firstInitial = normalizedInitials[0].ToString();
            var lastInitial = normalizedInitials[normalizedInitials.Length - 1].ToString();

            // Parent-i tap
            var parent = await _context.Parents
            .AsNoTracking()
            .Include(p => p.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(p =>
                p.FinCode == normalizedFin &&
                p.FirstName.StartsWith(firstInitial, StringComparison.OrdinalIgnoreCase) &&
                p.LastName.StartsWith(lastInitial, StringComparison.OrdinalIgnoreCase) &&
                p.IsActive &&
                !p.IsDeleted &&
                p.User!.IsActive &&
                !p.User.IsDeleted);

            if (parent == null)
                return (false, "FIN kod və ya ad/soyad yanlışdır", null);

            var user = parent.User;

            // ✅ Lockout yoxlaması
            var lockoutCheck = CheckLockout(user);
            if (!lockoutCheck.Success)
                return lockoutCheck;

            // Parent rolunu yoxla
            var hasParentRole = user.UserRoles.Any(ur =>
                ur.Role.IsActive && ur.Role.Code == "PARENT");

            if (!hasParentRole)
                return (false, "Bu istifadəçinin valideyn girişi yoxdur", null);

            // ✅ Uğurlu giriş
            await UpdateSuccessfulLoginAsync(user);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parent authentication xətası");
            return (false, "Sistemdə xəta baş verdi", null);
        }
    }

    #endregion

    #region Claims Principal

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user, Guid? companyId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", $"{user.FirstName} {user.LastName}")
        };

        if (companyId.HasValue && companyId != Guid.Empty)
        {
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
        }

        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == user.Id && ur.Role.IsActive)
            .ToListAsync();

        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Code ?? userRole.Role.Name));
        }

        var identity = new ClaimsIdentity(claims, "Cookie");
        return new ClaimsPrincipal(identity);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Lockout yoxlaması
    /// </summary>
    private (bool Success, string? ErrorMessage, User? User) CheckLockout(User user)
    {
        if (user.IsLocked && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return (false, $"Hesabınız {user.LockoutEnd:dd.MM.yyyy HH:mm} tarixinədək kilidlənib", null);
        }
        return (true, null, user);
    }

    /// <summary>
    /// Şifrə yoxlaması və failed attempts
    /// </summary>
    private async Task<(bool Success, string? ErrorMessage, User? User)> ValidatePasswordAsync(User user, string password)
    {
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
            }
            await _context.SaveChangesAsync();

            return (false, "Şifrə yanlışdır", null);
        }
        return (true, null, user);
    }

    /// <summary>
    /// Uğurlu giriş məlumatlarını yenilə
    /// </summary>
    private async Task UpdateSuccessfulLoginAsync(User user)
    {
        user.LastLoginDate = DateTimeOffset.UtcNow;
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEnd = null;
        await _context.SaveChangesAsync();
    }

    #endregion
}