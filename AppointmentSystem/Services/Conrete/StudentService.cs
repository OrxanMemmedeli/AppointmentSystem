using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Conrete;

/// <summary>
/// Şagird idarəetmə servisi implementasiyası
/// </summary>
public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentService> _logger;
    private readonly IWebHostEnvironment _environment;

    public StudentService(
        AppDbContext context,
        ILogger<StudentService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Bütün şagirdləri gətirir</summary>
    public async Task<List<StudentListViewModel>> GetAllStudentsAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(s => new StudentListViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                ClassId = s.ClassId,
                ClassName = s.Class.Name,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                IsActive = s.IsActive,
                ParentCount = s.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = s.CreatedDate
            })
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    /// <summary>Aktiv şagirdləri gətirir</summary>
    public async Task<List<StudentListViewModel>> GetActiveStudentsAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsActive)
            .Select(s => new StudentListViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                ClassId = s.ClassId,
                ClassName = s.Class.Name,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                IsActive = s.IsActive,
                ParentCount = s.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = s.CreatedDate
            })
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    /// <summary>Şirkətə görə şagirdləri gətirir</summary>
    public async Task<List<StudentListViewModel>> GetStudentsByCompanyAsync(Guid companyId)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.CompanyId == companyId)
            .Select(s => new StudentListViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                ClassId = s.ClassId,
                ClassName = s.Class.Name,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                IsActive = s.IsActive,
                ParentCount = s.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = s.CreatedDate
            })
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    /// <summary>Sinfə görə şagirdləri gətirir</summary>
    public async Task<List<StudentListViewModel>> GetStudentsByClassAsync(Guid classId)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.ClassId == classId)
            .Select(s => new StudentListViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                ClassId = s.ClassId,
                ClassName = s.Class.Name,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                IsActive = s.IsActive,
                ParentCount = s.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = s.CreatedDate
            })
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    /// <summary>ID-yə görə şagird gətirir</summary>
    public async Task<StudentViewModel?> GetStudentByIdAsync(Guid id)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new StudentViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                Notes = s.Notes,
                ClassId = s.ClassId,
                CompanyId = s.CompanyId,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>ID-yə görə şagird detaylarını gətirir</summary>
    public async Task<StudentDetailsViewModel?> GetStudentDetailsByIdAsync(Guid id)
    {
        var studentDetails = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new StudentDetailsViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FinCode = s.FinCode,
                DateOfBirth = s.DateOfBirth.Date,
                ImagePath = s.ImagePath,
                Notes = s.Notes,
                ClassId = s.ClassId,
                ClassName = s.Class.Name,
                ClassLevel = s.Class.Level,
                ClassSection = s.Class.Section,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                IsActive = s.IsActive,
                ParentCount = s.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = s.CreatedDate,
                Parents = s.StudentParents
                    .Where(sp => !sp.IsDeleted)
                    .Select(sp => new StudentParentInfo
                    {
                        ParentId = sp.ParentId,
                        ParentName = sp.Parent.FirstName + " " + sp.Parent.LastName,
                        ParentFinCode = sp.Parent.FinCode,
                        ParentPhone = sp.Parent.PhoneNumber,
                        ParentEmail = sp.Parent.Email,
                        RelationType = sp.RelationType.ToString(),
                        IsPrimaryContact = sp.IsPrimaryContact
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return studentDetails;
    }

    /// <summary>FIN koda görə şagird gətirir</summary>
    public async Task<Student?> GetStudentByFinCodeAsync(string finCode)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.FinCode == finCode && !s.IsDeleted);
    }

    /// <summary>Şagirdin mövcudluğunu yoxlayır</summary>
    public async Task<bool> StudentExistsAsync(Guid id)
    {
        return await _context.Students
            .AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    /// <summary>Yeni şagird yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? StudentId)> CreateStudentAsync(
        StudentViewModel model,
        Guid currentUserId)
    {
        try
        {
            // FIN kod unikallığını yoxla
            var isUnique = await IsFinCodeUniqueAsync(model.FinCode);
            if (!isUnique)
            {
                return (false, "Bu FIN kod artıq istifadə olunur", null);
            }

            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                FinCode = model.FinCode.ToUpperInvariant().Trim(),
                DateOfBirth = model.DateOfBirth,
                Notes = model.Notes?.Trim(),
                ClassId = model.ClassId,
                CompanyId = model.CompanyId,
                ImagePath = null,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedById = currentUserId
            };

            // Şəkil yüklənməsi
            if (model.ImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadImageAsync(model.ImageFile, student.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    student.ImagePath = filePath;
                }
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni şagird yaradıldı: {StudentName} (ID: {StudentId})",
                student.FirstName + " " + student.LastName, student.Id);

            return (true, null, student.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird yaradılarkən xəta: {StudentName}",
                model.FirstName + " " + model.LastName);
            return (false, "Şagird yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Şagirdi yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateStudentAsync(
        StudentViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Şagird ID-si tələb olunur");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == model.Id.Value && !s.IsDeleted);

            if (student == null)
            {
                return (false, "Şagird tapılmadı");
            }

            // FIN kod unikallığını yoxla
            if (model.FinCode != student.FinCode)
            {
                var isUnique = await IsFinCodeUniqueAsync(model.FinCode, student.Id);
                if (!isUnique)
                {
                    return (false, "Bu FIN kod artıq istifadə olunur");
                }
            }

            student.FirstName = model.FirstName.Trim();
            student.LastName = model.LastName.Trim();
            student.FinCode = model.FinCode.ToUpperInvariant().Trim();
            student.DateOfBirth = model.DateOfBirth;
            student.Notes = model.Notes?.Trim();
            student.ClassId = model.ClassId;
            student.CompanyId = model.CompanyId;
            student.IsActive = model.IsActive;
            student.ModifiedDate = DateTime.UtcNow;
            student.ModifiedById = currentUserId;

            // Yeni şəkil yüklənməsi
            if (model.ImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadImageAsync(model.ImageFile, student.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    student.ImagePath = filePath;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şagird yeniləndi: {StudentName} (ID: {StudentId})",
                student.FirstName + " " + student.LastName, student.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird yenilənərkən xəta: ID {StudentId}", model.Id);
            return (false, "Şagird yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Şagird statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleStudentStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                return (false, "Şagird tapılmadı");
            }

            student.IsActive = !student.IsActive;
            student.ModifiedDate = DateTime.UtcNow;
            student.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şagird statusu dəyişdi: {StudentName} - Yeni status: {IsActive}",
                student.FirstName + " " + student.LastName, student.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird statusu dəyişərkən xəta: ID {StudentId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Şagirdi silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteStudentAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.StudentParents)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                return (false, "Şagird tapılmadı");
            }

            // Aktiv məlumatları yoxla
            var activeParentCount = student.StudentParents.Count(sp => !sp.IsDeleted);

            if (activeParentCount > 0)
            {
                return (false,
                    $"Bu şagirdə aid məlumatlar var ({activeParentCount} valideyn əlaqəsi). " +
                    "Əvvəlcə onları silin.");
            }

            student.IsDeleted = true;
            student.IsActive = false;
            student.ModifiedDate = DateTime.UtcNow;
            student.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Şagird silindi: {StudentName} (ID: {StudentId})",
                student.FirstName + " " + student.LastName, student.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird silinərkən xəta: ID {StudentId}", id);
            return (false, "Şagird silinərkən xəta baş verdi");
        }
    }

    /// <summary>Şəkil yükləyir</summary>
    public async Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid studentId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Fayl seçilməyib", null);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, "Yalnız şəkil faylları (jpg, png, gif) yüklənə bilər", null);
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, "Fayl ölçüsü 5MB-dan böyük ola bilməz", null);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "students");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{studentId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/students/{fileName}";

            _logger.LogInformation(
                "Şagird şəkli yükləndi: StudentId={StudentId}, Path={Path}",
                studentId, relativePath);

            return (true, null, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil yüklənərkən xəta: StudentId={StudentId}", studentId);
            return (false, "Şəkil yüklənərkən xəta baş verdi", null);
        }
    }

    /// <summary>Şəkli silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid studentId)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

            if (student == null || string.IsNullOrEmpty(student.ImagePath))
            {
                return (false, "Şəkil tapılmadı");
            }

            var filePath = Path.Combine(_environment.WebRootPath, student.ImagePath.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            student.ImagePath = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Şagird şəkli silindi: StudentId={StudentId}", studentId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil silinərkən xəta: StudentId={StudentId}", studentId);
            return (false, "Şəkil silinərkən xəta baş verdi");
        }
    }

    /// <summary>FIN kod unikallığını yoxlayır</summary>
    public async Task<bool> IsFinCodeUniqueAsync(string finCode, Guid? excludeId = null)
    {
        var normalizedFinCode = finCode.ToUpperInvariant().Trim();

        var query = _context.Students
            .Where(s => !s.IsDeleted && s.FinCode == normalizedFinCode);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>Şagird select list gətirir</summary>
    public async Task<List<SelectListItem>> GetStudentSelectListAsync(Guid? companyId = null)
    {
        var query = _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsActive);

        if (companyId.HasValue)
        {
            query = query.Where(s => s.CompanyId == companyId.Value);
        }

        return await query
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.FirstName + " " + s.LastName + " (" + s.Class.Name + ")"
            })
            .ToListAsync();
    }

    /// <summary>Şagirdin valideynlərini gətirir</summary>
    public async Task<List<StudentParentInfo>> GetStudentParentsAsync(Guid studentId)
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => !sp.IsDeleted && sp.StudentId == studentId)
            .Select(sp => new StudentParentInfo
            {
                ParentId = sp.ParentId,
                ParentName = sp.Parent.FirstName + " " + sp.Parent.LastName,
                ParentFinCode = sp.Parent.FinCode,
                ParentPhone = sp.Parent.PhoneNumber,
                ParentEmail = sp.Parent.Email,
                RelationType = sp.RelationType.ToString(),
                IsPrimaryContact = sp.IsPrimaryContact
            })
            .ToListAsync();
    }
}