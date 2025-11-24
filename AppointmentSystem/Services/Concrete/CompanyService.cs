using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.ViewModels;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Şirkət idarəetmə servisi implementasiyası
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyService> _logger;
    private readonly IWebHostEnvironment _environment;

    public CompanyService(
        AppDbContext context,
        ILogger<CompanyService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Bütün şirkətləri gətirir</summary>
    public async Task<List<CompanyListViewModel>> GetAllCompaniesAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Select(c => new CompanyListViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Email = c.Email,
                Phone = c.PhoneNumber,
                Address = c.Address,
                LogoPath = c.LogoPath,
                IsActive = c.IsActive,
                StudentCount = c.Students.Count(s => !s.IsDeleted && s.IsActive),
                TeacherCount = c.Teachers.Count(t => !t.IsDeleted && t.IsActive),
                ClassCount = c.Classes.Count(sc => !sc.IsDeleted && sc.IsActive),
                SubjectCount = c.Subjects.Count(s => !s.IsDeleted && s.IsActive),
                CreatedDate = c.CreatedDate
            })
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    /// <summary>Aktiv şirkətləri gətirir</summary>
    public async Task<List<CompanyListViewModel>> GetActiveCompaniesAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive)
            .Select(c => new CompanyListViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Email = c.Email,
                Phone = c.PhoneNumber,
                LogoPath = c.LogoPath,
                IsActive = c.IsActive,
                CreatedDate = c.CreatedDate
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>ID-yə görə şirkət gətirir</summary>
    public async Task<CompanyViewModel?> GetCompanyByIdAsync(Guid id)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CompanyViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                Website = c.Website,
                LogoPath = c.LogoPath,
                BackgroundImagePath = c.BackgroundImagePath,
                Description = c.Description,
                MapUrl = c.MapUrl,
                MapCoordinates = c.MapCoordinates,
                DefaultMeetingDuration = c.DefaultMeetingDuration,
                DefaultBreakDuration = c.DefaultBreakDuration,
                DefaultStartTime = c.DefaultStartTime,
                DefaultEndTime = c.DefaultEndTime,
                WorkingDays = c.WorkingDays,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>Koda görə şirkət gətirir</summary>
    public async Task<Company?> GetCompanyByCodeAsync(string code)
    {
        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
    }

    /// <summary>Yeni şirkət yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? CompanyId)> CreateCompanyAsync(
        CompanyViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Kod unikallığını yoxla
            var isUnique = await IsCodeUniqueAsync(model.Code);
            if (!isUnique)
            {
                return (false, "Bu kod artıq istifadə olunur", null);
            }

            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Code = model.Code.ToUpperInvariant().Trim(),
                Email = model.Email?.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                Address = model.Address?.Trim(),
                Website = model.Website?.Trim(),
                Description = model.Description?.Trim(),
                MapUrl = model.MapUrl?.Trim(),
                MapCoordinates = model.MapCoordinates?.Trim(),
                DefaultMeetingDuration = model.DefaultMeetingDuration,
                DefaultBreakDuration = model.DefaultBreakDuration,
                DefaultStartTime = model.DefaultStartTime,
                DefaultEndTime = model.DefaultEndTime,
                WorkingDays = model.WorkingDays,
                LogoPath = "images/default-company-logo.jpg",
                BackgroundImagePath = "images/default-company-background-logo.jpg",
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            // Logo yüklənməsi
            if (model.LogoFile != null)
            {
                var (success, errorMessage, filePath) = await UploadLogoAsync(model.LogoFile, company.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    company.LogoPath = filePath;
                }
            }

            // Background şəkil yüklənməsi
            if (model.BackgroundImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadBackgroundImageAsync(model.BackgroundImageFile, company.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    company.BackgroundImagePath = filePath;
                }
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni şirkət yaradıldı: {CompanyName} (ID: {CompanyId})",
                company.Name, company.Id);

            return (true, null, company.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət yaradılarkən xəta: {CompanyName}", model.Name);
            return (false, "Şirkət yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Şirkəti yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateCompanyAsync(
        CompanyViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Şirkət ID-si tələb olunur");
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == model.Id.Value && !c.IsDeleted);

            if (company == null)
            {
                return (false, "Şirkət tapılmadı");
            }

            // Kod unikallığını yoxla
            if (model.Code != company.Code)
            {
                var isUnique = await IsCodeUniqueAsync(model.Code, company.Id);
                if (!isUnique)
                {
                    return (false, "Bu kod artıq istifadə olunur");
                }
            }

            company.Name = model.Name.Trim();
            company.Code = model.Code.ToUpperInvariant().Trim();
            company.Email = model.Email?.Trim().ToLowerInvariant();
            company.PhoneNumber = model.PhoneNumber?.Trim();
            company.Address = model.Address?.Trim();
            company.Website = model.Website?.Trim();
            company.Description = model.Description?.Trim();
            company.MapUrl = model.MapUrl?.Trim();
            company.MapCoordinates = model.MapCoordinates?.Trim();
            company.DefaultMeetingDuration = model.DefaultMeetingDuration;
            company.DefaultBreakDuration = model.DefaultBreakDuration;
            company.DefaultStartTime = model.DefaultStartTime;
            company.DefaultEndTime = model.DefaultEndTime;
            company.WorkingDays = model.WorkingDays;
            company.IsActive = model.IsActive;
            company.ModifiedDate = DateTime.Now;
            company.ModifiedById = currentUserId;

            // Yeni logo yüklənməsi
            if (model.LogoFile != null)
            {
                var (success, errorMessage, filePath) = await UploadLogoAsync(model.LogoFile, company.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    company.LogoPath = filePath;
                }
            }

            // Yeni background şəkil yüklənməsi
            if (model.BackgroundImageFile != null)
            {
                var (success, errorMessage, filePath) = await UploadBackgroundImageAsync(model.BackgroundImageFile, company.Id);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    company.BackgroundImagePath = filePath;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkət yeniləndi: {CompanyName} (ID: {CompanyId})",
                company.Name, company.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət yenilənərkən xəta: ID {CompanyId}", model.Id);
            return (false, "Şirkət yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Şirkət statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleCompanyStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (company == null)
            {
                return (false, "Şirkət tapılmadı");
            }

            company.IsActive = !company.IsActive;
            company.ModifiedDate = DateTime.Now;
            company.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkət statusu dəyişdi: {CompanyName} - Yeni status: {IsActive}",
                company.Name, company.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət statusu dəyişərkən xəta: ID {CompanyId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Şirkəti silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteCompanyAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var company = await _context.Companies
                .Include(c => c.Students)
                .Include(c => c.Teachers)
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (company == null)
            {
                return (false, "Şirkət tapılmadı");
            }

            // Aktiv məlumatları yoxla
            var activeStudentCount = company.Students.Count(s => !s.IsDeleted);
            var activeTeacherCount = company.Teachers.Count(t => !t.IsDeleted);
            var activeClassCount = company.Classes.Count(sc => !sc.IsDeleted);

            if (activeStudentCount > 0 || activeTeacherCount > 0 || activeClassCount > 0)
            {
                return (false,
                    $"Bu şirkətə aid məlumatlar var ({activeStudentCount} şagird, {activeTeacherCount} müəllim, {activeClassCount} sinif). " +
                    "Əvvəlcə onları silin və ya başqa şirkətə köçürün.");
            }

            company.IsDeleted = true;
            company.IsActive = false;
            company.ModifiedDate = DateTime.Now;
            company.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Şirkət silindi: {CompanyName} (ID: {CompanyId})",
                company.Name, company.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət silinərkən xəta: ID {CompanyId}", id);
            return (false, "Şirkət silinərkən xəta baş verdi");
        }
    }

    /// <summary>Logo yükləyir</summary>
    public async Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadLogoAsync(
        IFormFile file,
        Guid companyId)
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

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "companies", "logos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{companyId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/companies/logos/{fileName}";

            _logger.LogInformation(
                "Logo yükləndi: CompanyId={CompanyId}, Path={Path}",
                companyId, relativePath);

            return (true, null, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logo yüklənərkən xəta: CompanyId={CompanyId}", companyId);
            return (false, "Logo yüklənərkən xəta baş verdi", null);
        }
    }

    /// <summary>Background şəkil yükləyir</summary>
    public async Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadBackgroundImageAsync(
        IFormFile file,
        Guid companyId)
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

            if (file.Length > 10 * 1024 * 1024)
            {
                return (false, "Fayl ölçüsü 10MB-dan böyük ola bilməz", null);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "companies", "backgrounds");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{companyId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/companies/backgrounds/{fileName}";

            _logger.LogInformation(
                "Background şəkil yükləndi: CompanyId={CompanyId}, Path={Path}",
                companyId, relativePath);

            return (true, null, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background şəkil yüklənərkən xəta: CompanyId={CompanyId}", companyId);
            return (false, "Background şəkil yüklənərkən xəta baş verdi", null);
        }
    }

    /// <summary>Logonu silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteLogoAsync(Guid companyId)
    {
        try
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);

            if (company == null || string.IsNullOrEmpty(company.LogoPath))
            {
                return (false, "Logo tapılmadı");
            }

            if (company.LogoPath != "images/default-company-logo.jpg")
            {
                var filePath = Path.Combine(_environment.WebRootPath, company.LogoPath.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            company.LogoPath = "images/default-company-logo.jpg";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logo silindi: CompanyId={CompanyId}", companyId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logo silinərkən xəta: CompanyId={CompanyId}", companyId);
            return (false, "Logo silinərkən xəta baş verdi");
        }
    }

    /// <summary>Background şəkli silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteBackgroundImageAsync(Guid companyId)
    {
        try
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);

            if (company == null || string.IsNullOrEmpty(company.BackgroundImagePath))
            {
                return (false, "Background şəkil tapılmadı");
            }

            if (company.BackgroundImagePath != "images/default-company-background-logo.jpg")
            {
                var filePath = Path.Combine(_environment.WebRootPath, company.BackgroundImagePath.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            company.BackgroundImagePath = "images/default-company-background-logo.jpg";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Background şəkil silindi: CompanyId={CompanyId}", companyId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background şəkil silinərkən xəta: CompanyId={CompanyId}", companyId);
            return (false, "Background şəkil silinərkən xəta baş verdi");
        }
    }

    /// <summary>Kod unikallığını yoxlayır</summary>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var normalizedCode = code.ToUpperInvariant().Trim();

        var query = _context.Companies
            .Where(c => !c.IsDeleted && c.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>Şirkət select list gətirir</summary>
    public async Task<List<SelectListItem>> GetCompanySelectListAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }

    /// <summary>Bütün aktiv şirkətləri gətirir (alias metod)</summary>
    public async Task<List<CompanyListViewModel>> GetAllActiveCompaniesAsync()
    {
        return await GetActiveCompaniesAsync();
    }

    /// <summary>ID-yə görə şirkət entity-si gətirir</summary>
    public async Task<Company?> GetCompanyEntityByIdAsync(Guid id)
    {
        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    /// <summary>Şirkətin mövcudluğunu yoxlayır</summary>
    public async Task<bool> CompanyExistsAsync(Guid id)
    {
        return await _context.Companies
            .AnyAsync(c => c.Id == id && !c.IsDeleted);
    }

    /// <summary>Şirkəti doğrulayır (admin üçün)</summary>
    public async Task<(bool Success, string? ErrorMessage)> VerifyCompanyAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (company == null)
            {
                return (false, "Şirkət tapılmadı");
            }

            // Əgər şirkətdə IsVerified field-i varsa (yoxdursa bu metod sadəcə log yazsın)
            // Hazırda Company entity-də IsVerified yoxdur, ona görə sadəcə log yazırıq

            company.ModifiedDate = DateTime.Now;
            company.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkət yoxlanıldı/təsdiq edildi: {CompanyName} (ID: {CompanyId}) - Admin: {UserId}",
                company.Name, company.Id, currentUserId);

            return (true, "Şirkət uğurla təsdiq edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət təsdiq edilərkən xəta: ID {CompanyId}", id);
            return (false, "Şirkət təsdiq edilərkən xəta baş verdi");
        }
    }

    /// <summary>
    /// ✅ YENİ METOD - Aktiv şirkətlərin kart məlumatlarını gətirir
    /// </summary>
    public async Task<List<CompanyCardViewModel>> GetCompanyCardsAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive && c.IsVerified)
            .Select(c => new CompanyCardViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                LogoPath = c.LogoPath,
                BackgroundImagePath = c.BackgroundImagePath,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                MapCoordinates = c.MapCoordinates,
                MapUrl = c.MapUrl
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }


    #region CompanySubject Management

    /// <summary>Şirkətin fənlərini gətirir</summary>
    public async Task<List<SubjectListViewModel>> GetCompanySubjectsAsync(Guid companyId)
    {
        return await _context.CompanySubjects
            .AsNoTracking()
            .Where(cs => cs.CompanyId == companyId && !cs.IsDeleted)
            .Select(cs => new SubjectListViewModel
            {
                Id = cs.SubjectId,
                Name = cs.Subject.Name,
                Code = cs.Subject.Code,
                Description = cs.Subject.Description,
                IsActive = cs.Subject.IsActive,
                TeacherCount = cs.Subject.TeacherSubjects.Count(ts => !ts.IsDeleted),
                CompanyCount = cs.Subject.CompanySubjects.Count(ccs => !ccs.IsDeleted),
                CreatedDate = cs.Subject.CreatedDate
            })
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>Şirkətə fənn əlavə edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignSubjectToCompanyAsync(
        Guid companyId,
        Guid subjectId,
        Guid currentUserId)
    {
        try
        {
            // Əlaqənin mövcudluğunu yoxla
            var exists = await _context.CompanySubjects
                .AnyAsync(cs => cs.CompanyId == companyId && cs.SubjectId == subjectId && !cs.IsDeleted);

            if (exists)
            {
                return (false, "Bu fənn artıq şirkətə əlavə edilib");
            }

            var companySubject = new CompanySubject
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                SubjectId = subjectId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.CompanySubjects.Add(companySubject);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkətə fənn əlavə edildi: CompanyId={CompanyId}, SubjectId={SubjectId}",
                companyId, subjectId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkətə fənn əlavə edilərkən xəta: CompanyId={CompanyId}, SubjectId={SubjectId}",
                companyId, subjectId);
            return (false, "Fənn əlavə edilərkən xəta baş verdi");
        }
    }

    /// <summary>Şirkətdən fənni çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveSubjectFromCompanyAsync(
        Guid companyId,
        Guid subjectId,
        Guid currentUserId)
    {
        try
        {
            var companySubject = await _context.CompanySubjects
                .FirstOrDefaultAsync(cs => cs.CompanyId == companyId && cs.SubjectId == subjectId && !cs.IsDeleted);

            if (companySubject == null)
            {
                return (false, "Bu fənn şirkətdə tapılmadı");
            }

            companySubject.IsDeleted = true;
            companySubject.ModifiedDate = DateTime.Now;
            companySubject.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkətdən fənn çıxarıldı: CompanyId={CompanyId}, SubjectId={SubjectId}",
                companyId, subjectId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkətdən fənn çıxarılarkən xəta: CompanyId={CompanyId}, SubjectId={SubjectId}",
                companyId, subjectId);
            return (false, "Fənn çıxarılarkən xəta baş verdi");
        }
    }

    /// <summary>Fənnin şirkətdə olub-olmadığını yoxlayır</summary>
    public async Task<bool> IsSubjectAssignedToCompanyAsync(Guid companyId, Guid subjectId)
    {
        return await _context.CompanySubjects
            .AnyAsync(cs => cs.CompanyId == companyId && cs.SubjectId == subjectId && !cs.IsDeleted);
    }

    #endregion

    #region CompanyUser Management

    /// <summary>Şirkətin istifadəçilərini gətirir</summary>
    public async Task<List<CompanyUserListViewModel>> GetCompanyUsersAsync(Guid companyId)
    {
        return await _context.CompanyUsers
            .AsNoTracking()
            .Where(cu => cu.CompanyId == companyId && !cu.IsDeleted)
            .Select(cu => new CompanyUserListViewModel
            {
                Id = cu.Id,
                CompanyId = cu.CompanyId,
                CompanyName = cu.Company.Name,
                UserId = cu.UserId,
                UserName = cu.User.UserName,
                UserFullName = cu.User.FirstName + " " + cu.User.LastName,
                UserEmail = cu.User.Email,
                IsManager = cu.IsManager,
                IsActive = cu.IsActive,
                CreatedDate = cu.CreatedDate
            })
            .OrderBy(cu => cu.UserFullName)
            .ToListAsync();
    }

    /// <summary>Şirkətə istifadəçi əlavə edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignUserToCompanyAsync(
        Guid companyId,
        Guid userId,
        bool isManager,
        Guid currentUserId)
    {
        try
        {
            // Əlaqənin mövcudluğunu yoxla
            var exists = await _context.CompanyUsers
                .AnyAsync(cu => cu.CompanyId == companyId && cu.UserId == userId && !cu.IsDeleted);

            if (exists)
            {
                return (false, "Bu istifadəçi artıq şirkətə əlavə edilib");
            }

            var companyUser = new CompanyUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                UserId = userId,
                IsManager = isManager,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.CompanyUsers.Add(companyUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkətə istifadəçi əlavə edildi: CompanyId={CompanyId}, UserId={UserId}, IsManager={IsManager}",
                companyId, userId, isManager);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkətə istifadəçi əlavə edilərkən xəta: CompanyId={CompanyId}, UserId={UserId}",
                companyId, userId);
            return (false, "İstifadəçi əlavə edilərkən xəta baş verdi");
        }
    }

    /// <summary>Şirkətdən istifadəçini çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveUserFromCompanyAsync(
        Guid companyId,
        Guid userId,
        Guid currentUserId)
    {
        try
        {
            var companyUser = await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.CompanyId == companyId && cu.UserId == userId && !cu.IsDeleted);

            if (companyUser == null)
            {
                return (false, "Bu istifadəçi şirkətdə tapılmadı");
            }

            companyUser.IsDeleted = true;
            companyUser.ModifiedDate = DateTime.Now;
            companyUser.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şirkətdən istifadəçi çıxarıldı: CompanyId={CompanyId}, UserId={UserId}",
                companyId, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkətdən istifadəçi çıxarılarkən xəta: CompanyId={CompanyId}, UserId={UserId}",
                companyId, userId);
            return (false, "İstifadəçi çıxarılarkən xəta baş verdi");
        }
    }

    /// <summary>İstifadəçinin manager statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleManagerStatusAsync(
        Guid companyId,
        Guid userId,
        Guid currentUserId)
    {
        try
        {
            var companyUser = await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.CompanyId == companyId && cu.UserId == userId && !cu.IsDeleted);

            if (companyUser == null)
            {
                return (false, "Bu istifadəçi şirkətdə tapılmadı");
            }

            companyUser.IsManager = !companyUser.IsManager;
            companyUser.ModifiedDate = DateTime.Now;
            companyUser.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "İstifadəçinin manager statusu dəyişdi: CompanyId={CompanyId}, UserId={UserId}, IsManager={IsManager}",
                companyId, userId, companyUser.IsManager);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manager statusu dəyişərkən xəta: CompanyId={CompanyId}, UserId={UserId}",
                companyId, userId);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>İstifadəçinin şirkətdə olub-olmadığını yoxlayır</summary>
    public async Task<bool> IsUserAssignedToCompanyAsync(Guid companyId, Guid userId)
    {
        return await _context.CompanyUsers
            .AnyAsync(cu => cu.CompanyId == companyId && cu.UserId == userId && !cu.IsDeleted);
    }

    #endregion
}