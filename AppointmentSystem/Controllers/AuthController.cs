using AppointmentSystem.Services;
using AppointmentSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using AS = AppointmentSystem.Services;

namespace AppointmentSystem.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
public class AuthController : Controller
{
    private readonly AS.IAuthenticationService _authService;
    private readonly ICompanyService _companyService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AS.IAuthenticationService authService,
        ICompanyService companyService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _companyService = companyService;
        _logger = logger;
    }

    #region Company Selection

    /// <summary>
    /// Şirkət seçimi səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SelectCompany(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var companies = await _companyService.GetAllActiveCompaniesAsync();
        return View(companies);
    }

    #endregion

    #region Parent Login

    /// <summary>
    /// Valideyn login səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ParentLogin(Guid companyId, string? returnUrl = null)
    {
        if (!await _companyService.CompanyExistsAsync(companyId))
            return RedirectToAction(nameof(SelectCompany));

        var company = await _companyService.GetCompanyByIdAsync(companyId);
        ViewData["Company"] = company;
        ViewData["ReturnUrl"] = returnUrl;

        return View(new ParentLoginViewModel { CompanyId = companyId, ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Valideyn login POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ParentLogin(ParentLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
            ViewData["Company"] = company;
            return View(model);
        }

        var (success, errorMessage, user) = await _authService.AuthenticateParentAsync(model);

        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Giriş uğursuz oldu");
            var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
            ViewData["Company"] = company;
            return View(model);
        }

        // Create claims and sign in
        var principal = await _authService.CreateClaimsPrincipalAsync(user, model.CompanyId);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        _logger.LogInformation("Valideyn girişi: {UserName}", user.UserName);

        return RedirectToLocal(model.ReturnUrl ?? Url.Action("Index", "Parent"));
    }

    #endregion

    #region Teacher Login

    /// <summary>
    /// Müəllim login səhifəsi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TeacherLogin(Guid companyId, string? returnUrl = null)
    {
        if (!await _companyService.CompanyExistsAsync(companyId))
            return RedirectToAction(nameof(SelectCompany));

        var company = await _companyService.GetCompanyByIdAsync(companyId);
        ViewData["Company"] = company;
        ViewData["ReturnUrl"] = returnUrl;

        return View(new TeacherLoginViewModel { CompanyId = companyId, ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Müəllim login POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TeacherLogin(TeacherLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
            ViewData["Company"] = company;
            return View(model);
        }

        var (success, errorMessage, user) = await _authService.AuthenticateTeacherAsync(model);

        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Email və ya şifrə yanlışdır");
            var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
            ViewData["Company"] = company;
            return View(model);
        }

        // Create claims and sign in
        var principal = await _authService.CreateClaimsPrincipalAsync(user, model.CompanyId);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 : 8)
            });

        _logger.LogInformation("Müəllim girişi: {UserName}", user.UserName);

        return RedirectToLocal(model.ReturnUrl ?? Url.Action("Index", "Teacher"));
    }

    #endregion

    #region Admin Login

    /// <summary>
    /// Admin login səhifəsi
    /// </summary>
    [HttpGet]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Admin login POST
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, errorMessage, user) = await _authService.AuthenticateAdminAsync(model);

        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "İstifadəçi adı və ya şifrə yanlışdır");
            return View(model);
        }

        // Create claims and sign in (CompanyId = Guid.Empty for SuperAdmin)
        var principal = await _authService.CreateClaimsPrincipalAsync(user, Guid.Empty);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 : 8)
            });

        _logger.LogInformation("Admin girişi: {UserName}", user.UserName);

        return RedirectToLocal(model.ReturnUrl ?? Url.Action("Index", "Dashboard", new { area = "Admin" }));
    }

    #endregion

    #region Logout

    /// <summary>
    /// Çıxış
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("İstifadəçi çıxış etdi");
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Helper Methods

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction("Index", "Home");
    }

    #endregion
}
