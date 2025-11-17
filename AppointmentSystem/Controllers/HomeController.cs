using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Role əsasında redirect
        if (User.IsInRole("SUPERADMIN"))
        {
            return RedirectToAction("Index", "SuperAdmin");
        }
        else if (User.IsInRole("MANAGER"))
        {
            return RedirectToAction("Index", "Manager");
        }
        else if (User.IsInRole("TEACHER"))
        {
            return RedirectToAction("Index", "Teacher");
        }
        else if (User.IsInRole("PARENT"))
        {
            return RedirectToAction("Index", "Parent");
        }

        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
