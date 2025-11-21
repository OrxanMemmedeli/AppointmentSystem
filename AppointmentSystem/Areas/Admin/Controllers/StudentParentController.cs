using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Models.Enums;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AppointmentSystem.Areas.Admin.Controllers;

/// <summary>
/// Şagird-Valideyn əlaqəsi idarəetmə controller
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN,STUDENT_MANAGER")]
public class StudentParentController : Controller
{
    private readonly IStudentParentService _studentParentService;
    private readonly IStudentService _studentService;
    private readonly IParentService _parentService;
    private readonly IParentTypeService _parentTypeService;
    private readonly ILogger<StudentParentController> _logger;

    public StudentParentController(
        IStudentParentService studentParentService,
        IStudentService studentService,
        IParentService parentService,
        IParentTypeService parentTypeService,
        ILogger<StudentParentController> logger)
    {
        _studentParentService = studentParentService;
        _studentService = studentService;
        _parentService = parentService;
        _parentTypeService = parentTypeService;
        _logger = logger;
    }

    /// <summary>Şagird-valideyn əlaqələri siyahısı</summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? studentId = null, Guid? parentId = null)
    {
        List<StudentParentListViewModel> relationships;

        if (studentId.HasValue)
        {
            relationships = await _studentParentService.GetParentsByStudentAsync(studentId.Value);
            ViewData["FilterStudentId"] = studentId.Value;
        }
        else if (parentId.HasValue)
        {
            relationships = await _studentParentService.GetStudentsByParentAsync(parentId.Value);
            ViewData["FilterParentId"] = parentId.Value;
        }
        else
        {
            relationships = await _studentParentService.GetAllStudentParentsAsync();
        }

        return View(relationships);
    }

    /// <summary>Şagird üçün valideyn əlavə et səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? studentId = null, Guid? parentId = null)
    {
        await PrepareViewDataAsync();

        var model = new StudentParentViewModel
        {
            StudentId = studentId ?? Guid.Empty,
            ParentId = parentId ?? Guid.Empty,
            IsActive = true,
            IsPrimaryContact = false
        };

        return View(model);
    }

    /// <summary>Yeni şagird-valideyn əlaqəsi yaradır</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentParentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, studentParentId) =
            await _studentParentService.CreateStudentParentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Əlaqə yaradılarkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Şagird-valideyn əlaqəsi uğurla yaradıldı";
        return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
    }

    /// <summary>Şagird-valideyn əlaqəsi redaktə səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var studentParent = await _studentParentService.GetStudentParentByIdAsync(id);
        if (studentParent == null)
        {
            TempData["ErrorMessage"] = "Əlaqə tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        await PrepareViewDataAsync();
        return View(studentParent);
    }

    /// <summary>Şagird-valideyn əlaqəsini yeniləyir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentParentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareViewDataAsync();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentParentService.UpdateStudentParentAsync(model, currentUserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Əlaqə yenilənərkən xəta baş verdi");
            await PrepareViewDataAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Şagird-valideyn əlaqəsi uğurla yeniləndi";
        return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
    }

    /// <summary>Əsas valideyni təyin edir</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryContact(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentParentService.SetPrimaryContactAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Əsas valideyn təyin edilərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Əsas valideyn uğurla təyin edildi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Şagird-valideyn əlaqəsi silmə təsdiq səhifəsi</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var studentParent = await _studentParentService.GetStudentParentByIdAsync(id);
        if (studentParent == null)
        {
            TempData["ErrorMessage"] = "Əlaqə tapılmadı";
            return RedirectToAction(nameof(Index));
        }

        return View(studentParent);
    }

    /// <summary>Şagird-valideyn əlaqəsini silir</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) =
            await _studentParentService.DeleteStudentParentAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Əlaqə silinərkən xəta baş verdi";
        }
        else
        {
            TempData["SuccessMessage"] = "Şagird-valideyn əlaqəsi uğurla silindi";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>ViewData hazırlayır</summary>
    private async Task PrepareViewDataAsync()
    {
        ViewData["Students"] = await _studentService.GetStudentSelectListAsync();
        ViewData["Parents"] = await _parentService.GetParentSelectListAsync();
        ViewData["ParentTypes"] = await _parentTypeService.GetParentTypeSelectListAsync();
        ViewData["RelationTypes"] = Enum.GetValues(typeof(ParentRelationType))
            .Cast<ParentRelationType>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = GetRelationTypeDisplay(e)
            })
            .ToList();
    }

    /// <summary>Qohumluq növünün göstərilməsi</summary>
    private string GetRelationTypeDisplay(ParentRelationType type)
    {
        return type switch
        {
            ParentRelationType.Father => "Ata",
            ParentRelationType.Mother => "Ana",
            ParentRelationType.Grandfather => "Baba",
            ParentRelationType.Grandmother => "Nənə",
            ParentRelationType.Brother => "Qardaş",
            ParentRelationType.Sister => "Bacı",
            ParentRelationType.Other => "Digər",
            _ => type.ToString()
        };
    }

    /// <summary>Cari istifadəçinin ID-sini gətirir</summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}