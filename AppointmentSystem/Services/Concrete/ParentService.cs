using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Valideyn idarəetmə servisi implementasiyası
/// </summary>
public class ParentService : IParentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ParentService> _logger;
    private readonly IWebHostEnvironment _environment;

    public ParentService(
        AppDbContext context,
        ILogger<ParentService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Bütün valideynləri gətirir</summary>
    public async Task<List<ParentListViewModel>> GetAllParentsAsync()
    {
        return await _context.Parents
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new ParentListViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FinCode = p.FinCode,
                Email = p.Email,
                Phone = p.PhoneNumber,
                ImagePath = p.ImagePath,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                IsActive = p.IsActive,
                ChildrenCount = p.StudentParents.Count(sp => !sp.IsDeleted),
                HasUser = p.UserId.HasValue,
                CreatedDate = p.CreatedDate
            })
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    /// <summary>Aktiv valideynləri gətirir</summary>
    public async Task<List<ParentListViewModel>> GetActiveParentsAsync()
    {
        return await _context.Parents
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .Select(p => new ParentListViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FinCode = p.FinCode,
                Email = p.Email,
                Phone = p.PhoneNumber,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                IsActive = p.IsActive,
                ChildrenCount = p.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = p.CreatedDate
            })
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    /// <summary>
    /// Şirkətə görə aktiv valideynləri gətirir (Entity)
    /// </summary>
    public async Task<List<Parent>> GetActiveParentsAsync(Guid? companyId)
    {
        try
        {
            var query = _context.Parents
                .AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(p => p.CompanyId == companyId.Value);
            }

            return await query
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiv valideynlər yüklənərkən xəta. CompanyId: {CompanyId}", companyId);
            return new List<Parent>();
        }
    }

    /// <summary>Şirkətə görə valideynləri gətirir</summary>
    public async Task<List<ParentListViewModel>> GetParentsByCompanyAsync(Guid companyId)
    {
        return await _context.Parents
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.CompanyId == companyId)
            .Select(p => new ParentListViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FinCode = p.FinCode,
                Email = p.Email,
                Phone = p.PhoneNumber,
                ImagePath = p.ImagePath,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                IsActive = p.IsActive,
                ChildrenCount = p.StudentParents.Count(sp => !sp.IsDeleted),
                HasUser = p.UserId.HasValue,
                CreatedDate = p.CreatedDate
            })
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    /// <summary>ID-yə görə valideyn gətirir</summary>
    public async Task<ParentViewModel?> GetParentByIdAsync(Guid id)
    {
        return await _context.Parents
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new ParentViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FinCode = p.FinCode,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                ImagePath = p.ImagePath,
                UserId = p.UserId,
                CompanyId = p.CompanyId,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>ID-yə görə valideyn detaylarını gətirir</summary>
    public async Task<ParentDetailsViewModel?> GetParentDetailsByIdAsync(Guid id)
    {
        var parentDetails = await _context.Parents
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new ParentDetailsViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FinCode = p.FinCode,
                Email = p.Email,
                Phone = p.PhoneNumber,
                PhoneNumber = p.PhoneNumber,
                ImagePath = p.ImagePath,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                UserId = p.UserId,
                UserName = p.User != null ? p.User.UserName : null,
                IsActive = p.IsActive,
                ChildrenCount = p.StudentParents.Count(sp => !sp.IsDeleted),
                MeetingCount = p.Meetings.Count(m => !m.IsDeleted),
                CreatedDate = p.CreatedDate,
                Children = p.StudentParents
                    .Where(sp => !sp.IsDeleted && !sp.Student.IsDeleted)
                    .Select(sp => new ParentChildInfo
                    {
                        StudentId = sp.StudentId,
                        StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                        ClassName = sp.Student.Class != null ? sp.Student.Class.Name : null,
                        RelationType = sp.RelationType.ToString(),
                        IsActive = sp.Student.IsActive
                    })
                    .ToList(),
                RecentMeetings = p.Meetings
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.MeetingDate)
                    .Take(5)
                    .Select(m => new ParentMeetingInfo
                    {
                        MeetingId = m.Id,
                        Title = m.Student.FirstName + " " + m.Student.LastName + " - " + m.Teacher.FirstName + " " + m.Teacher.LastName,
                        MeetingDate = m.MeetingDate,
                        TeacherName = m.Teacher != null ? m.Teacher.FirstName + " " + m.Teacher.LastName : null,
                        Status = m.Status.ToString()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return parentDetails;
    }

    /// <summary>FIN koda görə valideyn gətirir</summary>
    public async Task<Parent?> GetParentByFinCodeAsync(string finCode)
    {
        return await _context.Parents
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FinCode == finCode && !p.IsDeleted);
    }

    /// <summary>Valideyinin mövcudluğunu yoxlayır</summary>
    public async Task<bool> ParentExistsAsync(Guid id)
    {
        return await _context.Parents
            .AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    /// <summary>Yeni valideyn yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? ParentId)> CreateParentAsync(
        ParentViewModel model,
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

            var parent = new Parent
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                FinCode = model.FinCode.ToUpperInvariant().Trim(),
                Email = model.Email?.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                UserId = model.UserId,
                CompanyId = model.CompanyId,
                ImagePath = null,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            // Şəkil yüklənməsi
            if (model.ImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadImageAsync(model.ImageFile, parent.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    parent.ImagePath = filePath;
                }
            }

            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni valideyn yaradıldı: {ParentName} (ID: {ParentId})",
                parent.FirstName + " " + parent.LastName, parent.Id);

            return (true, null, parent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn yaradılarkən xəta: {ParentName}",
                model.FirstName + " " + model.LastName);
            return (false, "Valideyn yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Valideyni yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateParentAsync(
        ParentViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Valideyn ID-si tələb olunur");
            }

            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == model.Id.Value && !p.IsDeleted);

            if (parent == null)
            {
                return (false, "Valideyn tapılmadı");
            }

            // FIN kod unikallığını yoxla
            if (model.FinCode != parent.FinCode)
            {
                var isUnique = await IsFinCodeUniqueAsync(model.FinCode, parent.Id);
                if (!isUnique)
                {
                    return (false, "Bu FIN kod artıq istifadə olunur");
                }
            }

            parent.FirstName = model.FirstName.Trim();
            parent.LastName = model.LastName.Trim();
            parent.FinCode = model.FinCode.ToUpperInvariant().Trim();
            parent.Email = model.Email?.Trim().ToLowerInvariant();
            parent.PhoneNumber = model.Phone?.Trim();
            parent.PhoneNumber = model.PhoneNumber?.Trim();
            parent.UserId = model.UserId;
            parent.CompanyId = model.CompanyId;
            parent.IsActive = model.IsActive;
            parent.ModifiedDate = DateTime.Now;
            parent.ModifiedById = currentUserId;

            // Yeni şəkil yüklənməsi
            if (model.ImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadImageAsync(model.ImageFile, parent.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    parent.ImagePath = filePath;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Valideyn yeniləndi: {ParentName} (ID: {ParentId})",
                parent.FirstName + " " + parent.LastName, parent.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn yenilənərkən xəta: ID {ParentId}", model.Id);
            return (false, "Valideyn yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Valideyn statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleParentStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (parent == null)
            {
                return (false, "Valideyn tapılmadı");
            }

            parent.IsActive = !parent.IsActive;
            parent.ModifiedDate = DateTime.Now;
            parent.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Valideyn statusu dəyişdi: {ParentName} - Yeni status: {IsActive}",
                parent.FirstName + " " + parent.LastName, parent.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn statusu dəyişərkən xəta: ID {ParentId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Valideyni silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteParentAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var parent = await _context.Parents
                .Include(p => p.StudentParents)
                .Include(p => p.Meetings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (parent == null)
            {
                return (false, "Valideyn tapılmadı");
            }

            // Aktiv məlumatları yoxla
            var activeChildrenCount = parent.StudentParents.Count(sp => !sp.IsDeleted);
            var activeMeetingCount = parent.Meetings.Count(m => !m.IsDeleted);

            if (activeChildrenCount > 0 || activeMeetingCount > 0)
            {
                return (false,
                    $"Bu valideyinə aid məlumatlar var ({activeChildrenCount} uşaq, {activeMeetingCount} görüş). " +
                    "Əvvəlcə onları silin.");
            }

            parent.IsDeleted = true;
            parent.IsActive = false;
            parent.ModifiedDate = DateTime.Now;
            parent.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Valideyn silindi: {ParentName} (ID: {ParentId})",
                parent.FirstName + " " + parent.LastName, parent.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn silinərkən xəta: ID {ParentId}", id);
            return (false, "Valideyn silinərkən xəta baş verdi");
        }
    }

    /// <summary>Şəkil yükləyir</summary>
    public async Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid parentId)
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

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "parents");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{parentId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/parents/{fileName}";

            _logger.LogInformation(
                "Valideyn şəkli yükləndi: ParentId={ParentId}, Path={Path}",
                parentId, relativePath);

            return (true, null, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil yüklənərkən xəta: ParentId={ParentId}", parentId);
            return (false, "Şəkil yüklənərkən xəta baş verdi", null);
        }
    }

    /// <summary>Şəkli silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid parentId)
    {
        try
        {
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == parentId && !p.IsDeleted);

            if (parent == null || string.IsNullOrEmpty(parent.ImagePath))
            {
                return (false, "Şəkil tapılmadı");
            }

            var filePath = Path.Combine(_environment.WebRootPath, parent.ImagePath.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            parent.ImagePath = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Valideyn şəkli silindi: ParentId={ParentId}", parentId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil silinərkən xəta: ParentId={ParentId}", parentId);
            return (false, "Şəkil silinərkən xəta baş verdi");
        }
    }

    /// <summary>FIN kod unikallığını yoxlayır</summary>
    public async Task<bool> IsFinCodeUniqueAsync(string finCode, Guid? excludeId = null)
    {
        var normalizedFinCode = finCode.ToUpperInvariant().Trim();

        var query = _context.Parents
            .Where(p => !p.IsDeleted && p.FinCode == normalizedFinCode);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>Valideyn select list gətirir</summary>
    public async Task<List<SelectListItem>> GetParentSelectListAsync(Guid? companyId = null)
    {
        var query = _context.Parents
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive);

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        return await query
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FirstName + " " + p.LastName + " (" + p.FinCode + ")"
            })
            .ToListAsync();
    }

    /// <summary>Valideyinin uşaqlarını gətirir</summary>
    public async Task<List<ParentChildInfo>> GetParentChildrenAsync(Guid parentId)
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => !sp.IsDeleted && sp.ParentId == parentId && !sp.Student.IsDeleted)
            .Select(sp => new ParentChildInfo
            {
                StudentId = sp.StudentId,
                StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                ClassName = sp.Student.Class != null ? sp.Student.Class.Name : null,
                RelationType = sp.RelationType.ToString(),
                IsActive = sp.Student.IsActive
            })
            .ToListAsync();
    }
}