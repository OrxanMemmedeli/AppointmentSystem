using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Fənn idarəetmə controller
/// </summary>
[Area("Admin")]
public class SubjectController : Controller
{
    private readonly ISubjectService _subjectService;
    private readonly ILogger<SubjectController> _logger;

    public SubjectController(
        ISubjectService subjectService,
        ILogger<SubjectController> logger)
    {
        _subjectService = subjectService;
        _logger = logger;
    }

    #region CRUD Actions

    /// <summary>Fənn siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var subjects = await _subjectService.GetAllSubjectsAsync();
        return View(subjects);
    }

    /// <summary>Fənn detayları</summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var subject = await _subjectService.GetSubjectDetailsByIdAsync(id);
        if (subject == null)
        {
            TempData["ErrorMessage"] = "Fənn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(subject);
    }

    /// <summary>Yeni fənn yaratma səhifəsi</summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new SubjectViewModel { IsActive = true });
    }

    /// <summary>Yeni fənn yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, subjectId) =
            await _subjectService.CreateSubjectAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Fənn yaradılarkən xəta baş verdi");
            return View(model);
        }

        TempData["SuccessMessage"] = "Fənn uğurla yaradıldı";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Fənn redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var subject = await _subjectService.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            TempData["ErrorMessage"] = "Fənn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(subject);
    }

    /// <summary>Fənni yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SubjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _subjectService.UpdateSubjectAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Fənn yenilənərkən xəta baş verdi");
            return View(model);
        }

        TempData["SuccessMessage"] = "Fənn uğurla yeniləndi";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Fənn statusunu dəyişir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _subjectService.ToggleSubjectStatusAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Status dəyişərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Status uğurla dəyişdirildi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Fənn silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var subject = await _subjectService.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            TempData["ErrorMessage"] = "Fənn tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(subject);
    }

    /// <summary>Fənni silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _subjectService.DeleteSubjectAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Fənn silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Fənn uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Helper Methods

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}