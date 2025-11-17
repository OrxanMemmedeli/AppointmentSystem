using AppointmentSystem.Data;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Controllers;

/// <summary>
/// Şirkət seçimi controller
/// </summary>
public class CompanyController : Controller
{
    private readonly AppDbContext _context;

    public CompanyController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Bütün şirkətləri card view ilə göstərir
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var companies = await _context.Companies
            .Where(c => c.IsActive)
            .Select(c => new CompanyViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                LogoPath = c.LogoPath,
                BackgroundImagePath = c.BackgroundImagePath,
                MapUrl = c.MapUrl,
                Description = c.Description
            })
            .ToListAsync();

        return View(companies);
    }

    /// <summary>
    /// Şirkət seçimi - session-a yazır
    /// </summary>
    [HttpPost]
    public IActionResult Select(Guid companyId)
    {
        HttpContext.Session.SetString("SelectedCompanyId", companyId.ToString());
        return RedirectToAction("Login", "Account");
    }
}
