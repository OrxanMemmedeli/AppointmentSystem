using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Controllers;

public class HomeController : Controller
{
    /// <summary>
    /// Ana səhifə - Role-based redirect
    /// </summary>
    [Authorize]
    public IActionResult Index()
    {
        // ✅ Role əsasında AREA-ya redirect
        if (User.IsInRole("SUPERADMIN") || User.IsInRole("MANAGER"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        else if (User.IsInRole("TEACHER"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Teacher" });
        }
        else if (User.IsInRole("PARENT"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Parent" });
        }

        // Rol yoxdursa, çıxış et
        return RedirectToAction("Logout", "Auth");
    }

    /// <summary>
    /// Xəta səhifəsi
    /// </summary>
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }

    /// <summary>
    /// Giriş qadağan
    /// </summary>
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}