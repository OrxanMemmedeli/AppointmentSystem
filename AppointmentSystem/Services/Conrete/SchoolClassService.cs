using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Conrete;

/// <summary>
/// Sinif idarəetmə servisi implementasiyası
/// </summary>
public class SchoolClassService : ISchoolClassService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SchoolClassService> _logger;

    public SchoolClassService(
        AppDbContext context,
        ILogger<SchoolClassService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün sinifləri gətirir</summary>
    public async Task<List<SchoolClassListViewModel>> GetAllClassesAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted)
            .Select(sc => new SchoolClassListViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                Description = sc.Description,
                CompanyId = sc.CompanyId,
                CompanyName = sc.Company.Name,
                IsActive = sc.IsActive,
                StudentCount = sc.Students.Count(s => !s.IsDeleted && s.IsActive),
                TeacherCount = sc.TeacherClasses.Count(ct => !ct.IsDeleted && ct.Teacher.IsActive),
                CreatedDate = sc.CreatedDate
            })
            .OrderBy(sc => sc.CompanyName)
            .ThenBy(sc => sc.Level)
            .ThenBy(sc => sc.Section)
            .ToListAsync();
    }

    /// <summary>Aktiv sinifləri gətirir</summary>
    public async Task<List<SchoolClassListViewModel>> GetActiveClassesAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted && sc.IsActive)
            .Select(sc => new SchoolClassListViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                CompanyId = sc.CompanyId,
                CompanyName = sc.Company.Name,
                IsActive = sc.IsActive,
                StudentCount = sc.Students.Count(s => !s.IsDeleted && s.IsActive),
                CreatedDate = sc.CreatedDate
            })
            .OrderBy(sc => sc.Level)
            .ThenBy(sc => sc.Section)
            .ToListAsync();
    }

    /// <summary>Şirkətə görə sinifləri gətirir</summary>
    public async Task<List<SchoolClassListViewModel>> GetClassesByCompanyAsync(Guid companyId)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted && sc.CompanyId == companyId)
            .Select(sc => new SchoolClassListViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                Description = sc.Description,
                CompanyId = sc.CompanyId,
                CompanyName = sc.Company.Name,
                IsActive = sc.IsActive,
                StudentCount = sc.Students.Count(s => !s.IsDeleted && s.IsActive),
                TeacherCount = sc.TeacherClasses.Count(ct => !ct.IsDeleted && ct.Teacher.IsActive),
                CreatedDate = sc.CreatedDate
            })
            .OrderBy(sc => sc.Level)
            .ThenBy(sc => sc.Section)
            .ToListAsync();
    }

    /// <summary>ID-yə görə sinif gətirir</summary>
    public async Task<SchoolClassViewModel?> GetClassByIdAsync(Guid id)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(sc => sc.Id == id && !sc.IsDeleted)
            .Select(sc => new SchoolClassViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                Description = sc.Description,
                CompanyId = sc.CompanyId,
                IsActive = sc.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>ID-yə görə sinif detaylarını gətirir</summary>
    public async Task<SchoolClassDetailsViewModel?> GetClassDetailsByIdAsync(Guid id)
    {
        var classDetails = await _context.Classes
            .AsNoTracking()
            .Where(sc => sc.Id == id && !sc.IsDeleted)
            .Select(sc => new SchoolClassDetailsViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                Description = sc.Description,
                CompanyId = sc.CompanyId,
                CompanyName = sc.Company.Name,
                IsActive = sc.IsActive,
                StudentCount = sc.Students.Count(s => !s.IsDeleted && s.IsActive),
                TeacherCount = sc.TeacherClasses.Count(ct => !ct.IsDeleted && ct.Teacher.IsActive),
                CreatedDate = sc.CreatedDate,
                Students = sc.Students
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new StudentBasicInfo
                    {
                        Id = s.Id,
                        FullName = s.FirstName + " " + s.LastName,
                        IsActive = s.IsActive
                    })
                    .ToList(),
                Teachers = sc.TeacherClasses
                    .Where(ct => !ct.IsDeleted && ct.Teacher.IsActive)
                    .Select(ct => new TeacherBasicInfo
                    {
                        Id = ct.Teacher.Id,
                        FullName = ct.Teacher.FirstName + " " + ct.Teacher.LastName,
                        Email = ct.Teacher.Email,
                        IsActive = ct.Teacher.IsActive
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return classDetails;
    }

    /// <summary>Sinfin mövcudluğunu yoxlayır</summary>
    public async Task<bool> ClassExistsAsync(Guid id)
    {
        return await _context.Classes
            .AnyAsync(sc => sc.Id == id && !sc.IsDeleted);
    }

    /// <summary>Yeni sinif yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? ClassId)> CreateClassAsync(
        SchoolClassViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Sinif adının unikallığını yoxla
            var isUnique = await IsClassNameUniqueAsync(model.Name, model.CompanyId);
            if (!isUnique)
            {
                return (false, "Bu adda sinif artıq mövcuddur", null);
            }

            var schoolClass = new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Level = model.Level,
                Section = model.Section?.ToUpperInvariant().Trim(),
                Description = model.Description?.Trim(),
                CompanyId = model.CompanyId,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.Classes.Add(schoolClass);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni sinif yaradıldı: {ClassName} (ID: {ClassId})",
                schoolClass.Name, schoolClass.Id);

            return (true, null, schoolClass.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif yaradılarkən xəta: {ClassName}", model.Name);
            return (false, "Sinif yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Sinfi yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateClassAsync(
        SchoolClassViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Sinif ID-si tələb olunur");
            }

            var schoolClass = await _context.Classes
                .FirstOrDefaultAsync(sc => sc.Id == model.Id.Value && !sc.IsDeleted);

            if (schoolClass == null)
            {
                return (false, "Sinif tapılmadı");
            }

            // Sinif adının unikallığını yoxla
            if (model.Name != schoolClass.Name || model.CompanyId != schoolClass.CompanyId)
            {
                var isUnique = await IsClassNameUniqueAsync(model.Name, model.CompanyId, schoolClass.Id);
                if (!isUnique)
                {
                    return (false, "Bu adda sinif artıq mövcuddur");
                }
            }

            schoolClass.Name = model.Name.Trim();
            schoolClass.Level = model.Level;
            schoolClass.Section = model.Section?.ToUpperInvariant().Trim();
            schoolClass.Description = model.Description?.Trim();
            schoolClass.CompanyId = model.CompanyId;
            schoolClass.IsActive = model.IsActive;
            schoolClass.ModifiedDate = DateTime.Now;
            schoolClass.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Sinif yeniləndi: {ClassName} (ID: {ClassId})",
                schoolClass.Name, schoolClass.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif yenilənərkən xəta: ID {ClassId}", model.Id);
            return (false, "Sinif yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Sinif statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleClassStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var schoolClass = await _context.Classes
                .FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);

            if (schoolClass == null)
            {
                return (false, "Sinif tapılmadı");
            }

            schoolClass.IsActive = !schoolClass.IsActive;
            schoolClass.ModifiedDate = DateTime.Now;
            schoolClass.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Sinif statusu dəyişdi: {ClassName} - Yeni status: {IsActive}",
                schoolClass.Name, schoolClass.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif statusu dəyişərkən xəta: ID {ClassId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Sinfi silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteClassAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var schoolClass = await _context.Classes
                .Include(sc => sc.Students)
                .Include(sc => sc.TeacherClasses)
                .FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);

            if (schoolClass == null)
            {
                return (false, "Sinif tapılmadı");
            }

            // Aktiv məlumatları yoxla
            var activeStudentCount = schoolClass.Students.Count(s => !s.IsDeleted);
            var activeTeacherCount = schoolClass.TeacherClasses.Count(ct => !ct.IsDeleted);

            if (activeStudentCount > 0 || activeTeacherCount > 0)
            {
                return (false,
                    $"Bu sinfə aid məlumatlar var ({activeStudentCount} şagird, {activeTeacherCount} müəllim təyini). " +
                    "Əvvəlcə onları silin və ya başqa sinfə köçürün.");
            }

            schoolClass.IsDeleted = true;
            schoolClass.IsActive = false;
            schoolClass.ModifiedDate = DateTime.Now;
            schoolClass.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Sinif silindi: {ClassName} (ID: {ClassId})",
                schoolClass.Name, schoolClass.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif silinərkən xəta: ID {ClassId}", id);
            return (false, "Sinif silinərkən xəta baş verdi");
        }
    }

    /// <summary>Sinif adının unikallığını yoxlayır (şirkət daxilində)</summary>
    public async Task<bool> IsClassNameUniqueAsync(string name, Guid companyId, Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = _context.Classes
            .Where(sc => !sc.IsDeleted &&
                         sc.CompanyId == companyId &&
                         sc.Name == normalizedName);

        if (excludeId.HasValue)
        {
            query = query.Where(sc => sc.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>Sinif select list gətirir</summary>
    public async Task<List<SelectListItem>> GetClassSelectListAsync(Guid? companyId = null)
    {
        var query = _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted && sc.IsActive);

        if (companyId.HasValue)
        {
            query = query.Where(sc => sc.CompanyId == companyId.Value);
        }

        return await query
            .OrderBy(sc => sc.Level)
            .ThenBy(sc => sc.Section)
            .Select(sc => new SelectListItem
            {
                Value = sc.Id.ToString(),
                Text = sc.Name
            })
            .ToListAsync();
    }


    /// <summary>Sinif select list gətirir</summary>
    public async Task<List<SelectListItem>> GetSchoolClassSelectListAsync(Guid? companyId = null)
    {
        var query = _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted && sc.IsActive);

        if (companyId.HasValue)
        {
            query = query.Where(sc => sc.CompanyId == companyId.Value);
        }

        return await query
            .OrderBy(sc => sc.Level)
            .ThenBy(sc => sc.Section)
            .Select(sc => new SelectListItem
            {
                Value = sc.Id.ToString(),
                Text = sc.Name
            })
            .ToListAsync();
    }

    /// <summary>Səviyyəyə görə sinifləri gətirir</summary>
    public async Task<List<SchoolClassListViewModel>> GetClassesByLevelAsync(int level, Guid? companyId = null)
    {
        var query = _context.Classes
            .AsNoTracking()
            .Where(sc => !sc.IsDeleted && sc.Level == level);

        if (companyId.HasValue)
        {
            query = query.Where(sc => sc.CompanyId == companyId.Value);
        }

        return await query
            .Select(sc => new SchoolClassListViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                Level = sc.Level,
                Section = sc.Section,
                CompanyId = sc.CompanyId,
                CompanyName = sc.Company.Name,
                IsActive = sc.IsActive,
                StudentCount = sc.Students.Count(s => !s.IsDeleted && s.IsActive),
                CreatedDate = sc.CreatedDate
            })
            .OrderBy(sc => sc.Section)
            .ToListAsync();
    }
}
