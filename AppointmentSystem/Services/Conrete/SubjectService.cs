using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Fənn servisi implementasiyası
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(
        AppDbContext context,
        ILogger<SubjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Query Methods

    /// <summary>Bütün fənləri gətirir</summary>
    public async Task<List<SubjectListViewModel>> GetAllSubjectsAsync()
    {
        try
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .Select(s => new SubjectListViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    TeacherCount = s.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    CompanyCount = s.CompanySubjects.Count(cs => !cs.IsDeleted),
                    CreatedDate = s.CreatedDate
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənlər yüklənərkən xəta");
            return new List<SubjectListViewModel>();
        }
    }

    /// <summary>Aktiv fənləri gətirir</summary>
    public async Task<List<SubjectListViewModel>> GetActiveSubjectsAsync()
    {
        try
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted)
                .Select(s => new SubjectListViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    TeacherCount = s.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    CompanyCount = s.CompanySubjects.Count(cs => !cs.IsDeleted),
                    CreatedDate = s.CreatedDate
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiv fənlər yüklənərkən xəta");
            return new List<SubjectListViewModel>();
        }
    }

    /// <summary>ID-yə görə fənn gətirir</summary>
    public async Task<SubjectViewModel?> GetSubjectByIdAsync(Guid id)
    {
        try
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new SubjectViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn yüklənərkən xəta: {SubjectId}", id);
            return null;
        }
    }

    /// <summary>ID-yə görə fənn detaylarını gətirir</summary>
    public async Task<SubjectDetailsViewModel?> GetSubjectDetailsByIdAsync(Guid id)
    {
        try
        {
            var subject = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new SubjectDetailsViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedDate = s.CreatedDate,
                    ModifiedDate = s.ModifiedDate,
                    TeacherCount = s.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    CompanyCount = s.CompanySubjects.Count(cs => !cs.IsDeleted)
                })
                .FirstOrDefaultAsync();

            if (subject == null) return null;

            // Müəllimləri yüklə
            subject.Teachers = await GetSubjectTeachersAsync(id);

            // Şirkətləri yüklə
            subject.Companies = await GetSubjectCompaniesAsync(id);

            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn detayları yüklənərkən xəta: {SubjectId}", id);
            return null;
        }
    }

    /// <summary>Koda görə fənn gətirir</summary>
    public async Task<Subject?> GetSubjectByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        try
        {
            return await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Code == code && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn koda görə yüklənərkən xəta: {Code}", code);
            return null;
        }
    }

    /// <summary>Fənnin mövcudluğunu yoxlayır</summary>
    public async Task<bool> SubjectExistsAsync(Guid id)
    {
        try
        {
            return await _context.Subjects
                .AnyAsync(s => s.Id == id && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn mövcudluğu yoxlanarkən xəta: {SubjectId}", id);
            return false;
        }
    }

    /// <summary>Fənn select list gətirir</summary>
    public async Task<List<SelectListItem>> GetSubjectSelectListAsync()
    {
        try
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = !string.IsNullOrEmpty(s.Code)
                        ? $"{s.Name} ({s.Code})"
                        : s.Name
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn select list yüklənərkən xəta");
            return new List<SelectListItem>();
        }
    }

    #endregion

    #region Command Methods

    /// <summary>Yeni fənn yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? SubjectId)> CreateSubjectAsync(
        SubjectViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Validation
            if (!await IsNameUniqueAsync(model.Name))
            {
                return (false, "Bu adda fənn artıq mövcuddur", null);
            }

            if (!string.IsNullOrWhiteSpace(model.Code) && !await IsCodeUniqueAsync(model.Code))
            {
                return (false, "Bu kodda fənn artıq mövcuddur", null);
            }

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Code = model.Code?.Trim().ToUpperInvariant(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni fənn yaradıldı: {SubjectId} - {SubjectName} (User: {UserId})",
                subject.Id, subject.Name, currentUserId);

            return (true, null, subject.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn yaradılarkən xəta: {SubjectName}", model.Name);
            return (false, "Fənn yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Fənni yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateSubjectAsync(
        SubjectViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Fənn ID-si tapılmadı");
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == model.Id.Value && !s.IsDeleted);

            if (subject == null)
            {
                return (false, "Fənn tapılmadı");
            }

            // Validation
            if (!await IsNameUniqueAsync(model.Name, model.Id.Value))
            {
                return (false, "Bu adda fənn artıq mövcuddur");
            }

            if (!string.IsNullOrWhiteSpace(model.Code) && !await IsCodeUniqueAsync(model.Code, model.Id.Value))
            {
                return (false, "Bu kodda fənn artıq mövcuddur");
            }

            subject.Name = model.Name.Trim();
            subject.Code = model.Code?.Trim().ToUpperInvariant();
            subject.Description = model.Description?.Trim();
            subject.IsActive = model.IsActive;
            subject.ModifiedDate = DateTime.Now;
            subject.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Fənn yeniləndi: {SubjectId} - {SubjectName} (User: {UserId})",
                subject.Id, subject.Name, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn yenilənərkən xəta: {SubjectId}", model.Id);
            return (false, "Fənn yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Fənn statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleSubjectStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (subject == null)
            {
                return (false, "Fənn tapılmadı");
            }

            subject.IsActive = !subject.IsActive;
            subject.ModifiedDate = DateTime.Now;
            subject.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Fənn statusu dəyişdirildi: {SubjectId} - {IsActive} (User: {UserId})",
                subject.Id, subject.IsActive, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn statusu dəyişərkən xəta: {SubjectId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Fənni silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteSubjectAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (subject == null)
            {
                return (false, "Fənn tapılmadı");
            }

            // Soft delete
            subject.IsDeleted = true;
            subject.IsActive = false;
            subject.ModifiedDate = DateTime.Now;
            subject.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Fənn silindi: {SubjectId} - {SubjectName} (User: {UserId})",
                subject.Id, subject.Name, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn silinərkən xəta: {SubjectId}", id);
            return (false, "Fənn silinərkən xəta baş verdi");
        }
    }

    #endregion

    #region Validation Methods

    /// <summary>Ad unikallığını yoxlayır</summary>
    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        try
        {
            var query = _context.Subjects
                .Where(s => !s.IsDeleted && s.Name.ToLower() == name.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ad unikallığı yoxlanarkən xəta: {Name}", name);
            return false;
        }
    }

    /// <summary>Kod unikallığını yoxlayır</summary>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true; // Kod optional olduğu üçün boş ola bilər

        try
        {
            var query = _context.Subjects
                .Where(s => !s.IsDeleted && s.Code != null && s.Code.ToLower() == code.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kod unikallığı yoxlanarkən xəta: {Code}", code);
            return false;
        }
    }

    #endregion

    #region Statistics Methods

    /// <summary>Fənnə aid müəllimləri gətirir</summary>
    public async Task<List<TeacherListViewModel>> GetSubjectTeachersAsync(Guid subjectId)
    {
        try
        {
            return await _context.TeacherSubjects
                .AsNoTracking()
                .Where(ts => ts.SubjectId == subjectId && !ts.IsDeleted)
                .Select(ts => new TeacherListViewModel
                {
                    Id = ts.Teacher.Id,
                    FirstName = ts.Teacher.FirstName,
                    LastName = ts.Teacher.LastName,
                    FullName = ts.Teacher.FirstName + " " + ts.Teacher.LastName,
                    Email = ts.Teacher.Email,
                    PhoneNumber = ts.Teacher.PhoneNumber,
                    CompanyName = ts.Teacher.Company.Name,
                    IsActive = ts.Teacher.IsActive
                })
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn müəllimləri yüklənərkən xəta: {SubjectId}", subjectId);
            return new List<TeacherListViewModel>();
        }
    }

    /// <summary>Fənnə aid şirkətləri gətirir</summary>
    public async Task<List<CompanyListViewModel>> GetSubjectCompaniesAsync(Guid subjectId)
    {
        try
        {
            return await _context.CompanySubjects
                .AsNoTracking()
                .Where(cs => cs.SubjectId == subjectId && !cs.IsDeleted)
                .Select(cs => new CompanyListViewModel
                {
                    Id = cs.Company.Id,
                    Name = cs.Company.Name,
                    Code = cs.Company.Code,
                    Email = cs.Company.Email,
                    Phone = cs.Company.PhoneNumber,
                    Address = cs.Company.Address,
                    LogoPath = cs.Company.LogoPath,
                    IsActive = cs.Company.IsActive,
                    StudentCount = cs.Company.Students.Count(s => !s.IsDeleted),
                    TeacherCount = cs.Company.Teachers.Count(t => !t.IsDeleted),
                    ClassCount = cs.Company.Classes.Count(c => !c.IsDeleted),
                    SubjectCount = cs.Company.CompanySubjects.Count(ccs => !ccs.IsDeleted)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn şirkətləri yüklənərkən xəta: {SubjectId}", subjectId);
            return new List<CompanyListViewModel>();
        }
    }

    #endregion
}