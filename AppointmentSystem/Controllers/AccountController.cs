using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AppointmentSystem.Data;
using AppointmentSystem.Models.ViewModels;

namespace AppointmentSystem.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Email və ya Username ilə user tap
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => 
                (u.Email == model.EmailOrUsername || u.UserName == model.EmailOrUsername)
                && u.IsActive && !u.IsDeleted);

        if (user == null)
        {
            ModelState.AddModelError("", "Email/İstifadəçi adı və ya şifrə yanlışdır");
            return View(model);
        }

        // Şifrə yoxlama
        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            // Uğursuz cəhd artır
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
            }
            await _context.SaveChangesAsync();
            
            ModelState.AddModelError("", "Email/İstifadəçi adı və ya şifrə yanlışdır");
            return View(model);
        }

        if (user.IsLocked && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError("", $"Hesabınız {user.LockoutEnd:dd.MM.yyyy HH:mm} tarixinədək kilidlənib");
            return View(model);
        }

        // Claims yaradın
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", $"{user.FirstName} {user.LastName}")
        };

        // Rolları əlavə edin
        foreach (var userRole in user.UserRoles.Where(ur => ur.Role.IsActive))
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Code ?? userRole.Role.Name));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Last login update
        user.LastLoginDate = DateTimeOffset.UtcNow;
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEnd = null;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Company");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
