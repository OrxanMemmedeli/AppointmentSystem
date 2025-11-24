using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Müəllim servisi implementasiyası
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeacherService> _logger;
    private readonly IWebHostEnvironment _environment;

    public TeacherService(
        AppDbContext context,
        ILogger<TeacherService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    #region Query Methods

    /// <summary>Bütün müəllimləri gətirir</summary>
    public async Task<List<TeacherListViewModel>> GetAllTeachersAsync()
    {
        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Include(t => t.Company)
                .Select(t => new TeacherListViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    ImagePath = t.ImagePath,
                    Specialization = t.Specialization,
                    CompanyId = t.CompanyId,
                    CompanyName = t.Company.Name,
                    IsActive = t.IsActive,
                    SubjectCount = t.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    ClassCount = t.TeacherClasses.Count(tc => !tc.IsDeleted),
                    CreatedDate = t.CreatedDate
                })
                .OrderBy(t => t.CompanyName)
                .ThenBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimlər yüklənərkən xəta");
            return new List<TeacherListViewModel>();
        }
    }

    /// <summary>Aktiv müəllimləri gətirir</summary>
    public async Task<List<TeacherListViewModel>> GetActiveTeachersAsync()
    {
        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .Include(t => t.Company)
                .Select(t => new TeacherListViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    ImagePath = t.ImagePath,
                    Specialization = t.Specialization,
                    CompanyId = t.CompanyId,
                    CompanyName = t.Company.Name,
                    IsActive = t.IsActive,
                    SubjectCount = t.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    ClassCount = t.TeacherClasses.Count(tc => !tc.IsDeleted),
                    CreatedDate = t.CreatedDate
                })
                .OrderBy(t => t.CompanyName)
                .ThenBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiv müəllimlər yüklənərkən xəta");
            return new List<TeacherListViewModel>();
        }
    }

    /// <summary>
    /// Şirkətə görə aktiv müəllimləri gətirir (Entity)
    /// </summary>
    public async Task<List<Teacher>> GetActiveTeachersAsync(Guid? companyId)
    {
        try
        {
            var query = _context.Teachers
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(t => t.CompanyId == companyId.Value);
            }

            return await query
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiv müəllimlər yüklənərkən xəta. CompanyId: {CompanyId}", companyId);
            return new List<Teacher>();
        }
    }

    /// <summary>Şirkətə görə müəllimləri gətirir</summary>
    public async Task<List<TeacherListViewModel>> GetTeachersByCompanyAsync(Guid companyId)
    {
        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                .Include(t => t.Company)
                .Select(t => new TeacherListViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    ImagePath = t.ImagePath,
                    Specialization = t.Specialization,
                    CompanyId = t.CompanyId,
                    CompanyName = t.Company.Name,
                    IsActive = t.IsActive,
                    SubjectCount = t.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    ClassCount = t.TeacherClasses.Count(tc => !tc.IsDeleted),
                    CreatedDate = t.CreatedDate
                })
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şirkət müəllimləri yüklənərkən xəta: {CompanyId}", companyId);
            return new List<TeacherListViewModel>();
        }
    }

    /// <summary>ID-yə görə müəllim gətirir</summary>
    public async Task<TeacherViewModel?> GetTeacherByIdAsync(Guid id)
    {
        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .Where(t => t.Id == id && !t.IsDeleted)
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    Specialization = t.Specialization,
                    Biography = t.Biography,
                    UserId = t.UserId,
                    CompanyId = t.CompanyId,
                    ImagePath = t.ImagePath,
                    IsActive = t.IsActive
                })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim yüklənərkən xəta: {TeacherId}", id);
            return null;
        }
    }

    /// <summary>ID-yə görə müəllim detaylarını gətirir</summary>
    public async Task<TeacherDetailsViewModel?> GetTeacherDetailsByIdAsync(Guid id)
    {
        try
        {
            var teacher = await _context.Teachers
                .AsNoTracking()
                .Where(t => t.Id == id && !t.IsDeleted)
                .Include(t => t.User)
                .Include(t => t.Company)
                .Select(t => new TeacherDetailsViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    ImagePath = t.ImagePath,
                    Specialization = t.Specialization,
                    Biography = t.Biography,
                    IsActive = t.IsActive,
                    CreatedDate = t.CreatedDate,
                    ModifiedDate = t.ModifiedDate,
                    UserId = t.UserId,
                    UserName = t.User.UserName,
                    CompanyId = t.CompanyId,
                    CompanyName = t.Company.Name,
                    SubjectCount = t.TeacherSubjects.Count(ts => !ts.IsDeleted),
                    ClassCount = t.TeacherClasses.Count(tc => !tc.IsDeleted),
                    MeetingCount = t.Meetings.Count(m => !m.IsDeleted)
                })
                .FirstOrDefaultAsync();

            if (teacher == null) return null;

            // Fənləri yüklə
            teacher.Subjects = await GetTeacherSubjectsAsync(id);

            // Sinifləri yüklə
            teacher.Classes = await GetTeacherClassesAsync(id);

            // Son görüşləri yüklə (yalnız 5 ədəd)
            teacher.RecentMeetings = await GetTeacherMeetingsAsync(id);

            return teacher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim detayları yüklənərkən xəta: {TeacherId}", id);
            return null;
        }
    }

    /// <summary>User ID-yə görə müəllim gətirir</summary>
    public async Task<Teacher?> GetTeacherByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User ID-yə görə müəllim yüklənərkən xəta: {UserId}", userId);
            return null;
        }
    }

    /// <summary>Email-ə görə müəllim gətirir</summary>
    public async Task<Teacher?> GetTeacherByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        try
        {
            return await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Email == email && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email-ə görə müəllim yüklənərkən xəta: {Email}", email);
            return null;
        }
    }

    /// <summary>Müəllimin mövcudluğunu yoxlayır</summary>
    public async Task<bool> TeacherExistsAsync(Guid id)
    {
        try
        {
            return await _context.Teachers
                .AnyAsync(t => t.Id == id && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim mövcudluğu yoxlanarkən xəta: {TeacherId}", id);
            return false;
        }
    }

    /// <summary>Müəllim select list gətirir</summary>
    public async Task<List<SelectListItem>> GetTeacherSelectListAsync(Guid? companyId = null)
    {
        try
        {
            var query = _context.Teachers
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(t => t.CompanyId == companyId.Value);
            }

            return await query
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName + " (" + t.Email + ")"
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim select list yüklənərkən xəta");
            return new List<SelectListItem>();
        }
    }

    #endregion

    #region Command Methods

    /// <summary>Yeni müəllim yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? TeacherId)> CreateTeacherAsync(
        TeacherViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Validation
            if (!await IsEmailUniqueAsync(model.Email))
            {
                return (false, "Bu email artıq istifadə olunub", null);
            }

            if (!await IsUserIdAvailableAsync(model.UserId))
            {
                return (false, "Bu istifadəçi artıq müəllim kimi qeydiyyatdan keçib", null);
            }

            // Şirkətin mövcudluğunu yoxla
            var companyExists = await _context.Companies
                .AnyAsync(c => c.Id == model.CompanyId && !c.IsDeleted);

            if (!companyExists)
            {
                return (false, "Şirkət tapılmadı", null);
            }

            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                Specialization = model.Specialization?.Trim(),
                Biography = model.Biography?.Trim(),
                UserId = model.UserId,
                CompanyId = model.CompanyId,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni müəllim yaradıldı: {TeacherId} - {TeacherName} (User: {UserId})",
                teacher.Id, teacher.FirstName + " " + teacher.LastName, currentUserId);

            return (true, null, teacher.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim yaradılarkən xəta: {Email}", model.Email);
            return (false, "Müəllim yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Müəllimi yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateTeacherAsync(
        TeacherViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Müəllim ID-si tapılmadı");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == model.Id.Value && !t.IsDeleted);

            if (teacher == null)
            {
                return (false, "Müəllim tapılmadı");
            }

            // Validation
            if (!await IsEmailUniqueAsync(model.Email, model.Id.Value))
            {
                return (false, "Bu email artıq istifadə olunub");
            }

            if (!await IsUserIdAvailableAsync(model.UserId, model.Id.Value))
            {
                return (false, "Bu istifadəçi artıq müəllim kimi qeydiyyatdan keçib");
            }

            teacher.FirstName = model.FirstName.Trim();
            teacher.LastName = model.LastName.Trim();
            teacher.Email = model.Email.Trim().ToLowerInvariant();
            teacher.PhoneNumber = model.PhoneNumber?.Trim();
            teacher.Specialization = model.Specialization?.Trim();
            teacher.Biography = model.Biography?.Trim();
            teacher.UserId = model.UserId;
            teacher.CompanyId = model.CompanyId;
            teacher.IsActive = model.IsActive;
            teacher.ModifiedDate = DateTime.Now;
            teacher.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllim yeniləndi: {TeacherId} - {TeacherName} (User: {UserId})",
                teacher.Id, teacher.FirstName + " " + teacher.LastName, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim yenilənərkən xəta: {TeacherId}", model.Id);
            return (false, "Müəllim yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Müəllim statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleTeacherStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (teacher == null)
            {
                return (false, "Müəllim tapılmadı");
            }

            teacher.IsActive = !teacher.IsActive;
            teacher.ModifiedDate = DateTime.Now;
            teacher.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllim statusu dəyişdirildi: {TeacherId} - {IsActive} (User: {UserId})",
                teacher.Id, teacher.IsActive, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim statusu dəyişərkən xəta: {TeacherId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Müəllimi silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteTeacherAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (teacher == null)
            {
                return (false, "Müəllim tapılmadı");
            }

            // Soft delete
            teacher.IsDeleted = true;
            teacher.IsActive = false;
            teacher.ModifiedDate = DateTime.Now;
            teacher.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllim silindi: {TeacherId} - {TeacherName} (User: {UserId})",
                teacher.Id, teacher.FirstName + " " + teacher.LastName, currentUserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim silinərkən xəta: {TeacherId}", id);
            return (false, "Müəllim silinərkən xəta baş verdi");
        }
    }

    /// <summary>Şəkil yükləyir</summary>
    public async Task<(bool Success, string? ErrorMessage, string? FilePath)> UploadImageAsync(
        IFormFile file,
        Guid teacherId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Fayl seçilməyib", null);
            }

            // Fayl növünü yoxla
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, "Yalnız JPG, PNG və GIF formatları dəstəklənir", null);
            }

            // Fayl ölçüsünü yoxla (5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, "Fayl ölçüsü maksimum 5MB ola bilər", null);
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted);

            if (teacher == null)
            {
                return (false, "Müəllim tapılmadı", null);
            }

            // Köhnə şəkli sil
            if (!string.IsNullOrEmpty(teacher.ImagePath))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, teacher.ImagePath.TrimStart('/'));
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            // Yeni fayl adı
            var fileName = $"{teacherId}_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "teachers");

            // Qovluğu yarat
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            // Faylı yaddaşa yaz
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/teachers/{fileName}";
            teacher.ImagePath = relativePath;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Müəllim şəkli yükləndi: {TeacherId} - {FilePath}", teacherId, relativePath);

            return (true, null, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil yüklənərkən xəta: {TeacherId}", teacherId);
            return (false, "Şəkil yüklənərkən xəta baş verdi", null);
        }
    }

    /// <summary>Şəkli silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteImageAsync(Guid teacherId)
    {
        try
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted);

            if (teacher == null)
            {
                return (false, "Müəllim tapılmadı");
            }

            if (string.IsNullOrEmpty(teacher.ImagePath))
            {
                return (false, "Şəkil mövcud deyil");
            }

            // Faylı sil
            var filePath = Path.Combine(_environment.WebRootPath, teacher.ImagePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            teacher.ImagePath = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Müəllim şəkli silindi: {TeacherId}", teacherId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil silinərkən xəta: {TeacherId}", teacherId);
            return (false, "Şəkil silinərkən xəta baş verdi");
        }
    }

    #endregion

    #region Validation Methods

    /// <summary>Email unikallığını yoxlayır</summary>
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var query = _context.Teachers
                .Where(t => !t.IsDeleted && t.Email.ToLower() == email.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email unikallığı yoxlanarkən xəta: {Email}", email);
            return false;
        }
    }

    /// <summary>User ID-nin istifadə olunub-olunmadığını yoxlayır</summary>
    public async Task<bool> IsUserIdAvailableAsync(Guid userId, Guid? excludeId = null)
    {
        try
        {
            var query = _context.Teachers
                .Where(t => !t.IsDeleted && t.UserId == userId);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User ID mövcudluğu yoxlanarkən xəta: {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region TeacherSubject Management

    /// <summary>Müəllimin fənlərini gətirir</summary>
    public async Task<List<SubjectListViewModel>> GetTeacherSubjectsAsync(Guid teacherId)
    {
        try
        {
            return await _context.TeacherSubjects
                .AsNoTracking()
                .Where(ts => ts.TeacherId == teacherId && !ts.IsDeleted)
                .Select(ts => new SubjectListViewModel
                {
                    Id = ts.Subject.Id,
                    Name = ts.Subject.Name,
                    Code = ts.Subject.Code,
                    Description = ts.Subject.Description,
                    IsActive = ts.Subject.IsActive,
                    TeacherCount = ts.Subject.TeacherSubjects.Count(t => !t.IsDeleted),
                    CompanyCount = ts.Subject.CompanySubjects.Count(c => !c.IsDeleted),
                    CreatedDate = ts.Subject.CreatedDate
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim fənləri yüklənərkən xəta: {TeacherId}", teacherId);
            return new List<SubjectListViewModel>();
        }
    }

    /// <summary>Müəllimə fənn əlavə edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignSubjectToTeacherAsync(
        Guid teacherId,
        Guid subjectId,
        Guid currentUserId)
    {
        try
        {
            // Artıq təyin olunub-olunmadığını yoxla
            if (await IsSubjectAssignedToTeacherAsync(teacherId, subjectId))
            {
                return (false, "Bu fənn artıq müəllimə təyin olunub");
            }

            var teacherSubject = new TeacherSubject
            {
                Id = Guid.NewGuid(),
                TeacherId = teacherId,
                SubjectId = subjectId,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.TeacherSubjects.Add(teacherSubject);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllimə fənn təyin edildi: Teacher={TeacherId}, Subject={SubjectId}",
                teacherId, subjectId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimə fənn təyin edilərkən xəta: Teacher={TeacherId}, Subject={SubjectId}",
                teacherId, subjectId);
            return (false, "Fənn təyin edilərkən xəta baş verdi");
        }
    }

    /// <summary>Müəllimdən fənni çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveSubjectFromTeacherAsync(
        Guid teacherId,
        Guid subjectId,
        Guid currentUserId)
    {
        try
        {
            var teacherSubject = await _context.TeacherSubjects
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.SubjectId == subjectId && !ts.IsDeleted);

            if (teacherSubject == null)
            {
                return (false, "Fənn təyinatı tapılmadı");
            }

            // Soft delete
            teacherSubject.IsDeleted = true;
            teacherSubject.ModifiedDate = DateTime.Now;
            teacherSubject.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllimdən fənn çıxarıldı: Teacher={TeacherId}, Subject={SubjectId}",
                teacherId, subjectId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimdən fənn çıxarılarkən xəta: Teacher={TeacherId}, Subject={SubjectId}",
                teacherId, subjectId);
            return (false, "Fənn çıxarılarkən xəta baş verdi");
        }
    }

    /// <summary>Fənnin müəllimə təyin olunub-olunmadığını yoxlayır</summary>
    public async Task<bool> IsSubjectAssignedToTeacherAsync(Guid teacherId, Guid subjectId)
    {
        try
        {
            return await _context.TeacherSubjects
                .AnyAsync(ts => ts.TeacherId == teacherId && ts.SubjectId == subjectId && !ts.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fənn təyinatı yoxlanarkən xəta: Teacher={TeacherId}, Subject={SubjectId}",
                teacherId, subjectId);
            return false;
        }
    }

    #endregion

    #region TeacherClass Management

    /// <summary>Müəllimin siniflərini gətirir</summary>
    public async Task<List<TeacherClassViewModel>> GetTeacherClassesAsync(Guid teacherId)
    {
        try
        {
            return await _context.TeacherClasses
                .AsNoTracking()
                .Where(tc => tc.TeacherId == teacherId && !tc.IsDeleted)
                .Select(tc => new TeacherClassViewModel
                {
                    Id = tc.Id,
                    TeacherId = tc.TeacherId,
                    TeacherName = tc.Teacher.FirstName + " " + tc.Teacher.LastName,
                    ClassId = tc.ClassId,
                    ClassName = tc.Class.Name,
                    SubjectId = tc.SubjectId,
                    SubjectName = tc.Subject != null ? tc.Subject.Name : null,
                    IsClassLeader = tc.IsClassLeader,
                    IsActive = tc.IsActive,
                    StudentCount = tc.Class.Students.Count(s => !s.IsDeleted),
                    CreatedDate = tc.CreatedDate
                })
                .OrderBy(tc => tc.ClassName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim sinifləri yüklənərkən xəta: {TeacherId}", teacherId);
            return new List<TeacherClassViewModel>();
        }
    }

    /// <summary>Müəllimə sinif əlavə edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> AssignClassToTeacherAsync(
        Guid teacherId,
        Guid classId,
        Guid? subjectId,
        bool isClassLeader,
        Guid currentUserId)
    {
        try
        {
            // Artıq təyin olunub-olunmadığını yoxla
            if (await IsClassAssignedToTeacherAsync(teacherId, classId))
            {
                return (false, "Bu sinif artıq müəllimə təyin olunub");
            }

            var teacherClass = new TeacherClass
            {
                Id = Guid.NewGuid(),
                TeacherId = teacherId,
                ClassId = classId,
                SubjectId = subjectId,
                IsClassLeader = isClassLeader,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId
            };

            _context.TeacherClasses.Add(teacherClass);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllimə sinif təyin edildi: Teacher={TeacherId}, Class={ClassId}, IsLeader={IsClassLeader}",
                teacherId, classId, isClassLeader);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimə sinif təyin edilərkən xəta: Teacher={TeacherId}, Class={ClassId}",
                teacherId, classId);
            return (false, "Sinif təyin edilərkən xəta baş verdi");
        }
    }

    /// <summary>Müəllimdən sinfi çıxarır</summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveClassFromTeacherAsync(
        Guid teacherId,
        Guid classId,
        Guid currentUserId)
    {
        try
        {
            var teacherClass = await _context.TeacherClasses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId && !tc.IsDeleted);

            if (teacherClass == null)
            {
                return (false, "Sinif təyinatı tapılmadı");
            }

            // Soft delete
            teacherClass.IsDeleted = true;
            teacherClass.ModifiedDate = DateTime.Now;
            teacherClass.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Müəllimdən sinif çıxarıldı: Teacher={TeacherId}, Class={ClassId}",
                teacherId, classId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllimdən sinif çıxarılarkən xəta: Teacher={TeacherId}, Class={ClassId}",
                teacherId, classId);
            return (false, "Sinif çıxarılarkən xəta baş verdi");
        }
    }

    /// <summary>Sinif rəhbəri statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleClassLeaderAsync(
        Guid teacherId,
        Guid classId,
        Guid currentUserId)
    {
        try
        {
            var teacherClass = await _context.TeacherClasses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId && !tc.IsDeleted);

            if (teacherClass == null)
            {
                return (false, "Sinif təyinatı tapılmadı");
            }

            teacherClass.IsClassLeader = !teacherClass.IsClassLeader;
            teacherClass.ModifiedDate = DateTime.Now;
            teacherClass.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Sinif rəhbəri statusu dəyişdirildi: Teacher={TeacherId}, Class={ClassId}, IsLeader={IsClassLeader}",
                teacherId, classId, teacherClass.IsClassLeader);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif rəhbəri statusu dəyişərkən xəta: Teacher={TeacherId}, Class={ClassId}",
                teacherId, classId);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Sinfin müəllimə təyin olunub-olunmadığını yoxlayır</summary>
    public async Task<bool> IsClassAssignedToTeacherAsync(Guid teacherId, Guid classId)
    {
        try
        {
            return await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId && !tc.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinif təyinatı yoxlanarkən xəta: Teacher={TeacherId}, Class={ClassId}",
                teacherId, classId);
            return false;
        }
    }

    #endregion

    #region Statistics Methods

    /// <summary>Müəllimin görüşlərini gətirir</summary>
    public async Task<List<MeetingListViewModel>> GetTeacherMeetingsAsync(Guid teacherId)
    {
        try
        {
            return await _context.Meetings
                .AsNoTracking()
                .Where(m => m.TeacherId == teacherId && !m.IsDeleted)
                .OrderByDescending(m => m.MeetingDate)
                .ThenByDescending(m => m.StartTime)
                .Take(5) // Yalnız son 5 görüş
                .Select(m => new MeetingListViewModel
                {
                    Id = m.Id,
                    MeetingDate = m.MeetingDate,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    TeacherId = m.TeacherId,
                    TeacherName = m.Teacher.FirstName + " " + m.Teacher.LastName,
                    ParentId = m.ParentId,
                    ParentName = m.Parent.FirstName + " " + m.Parent.LastName,
                    StudentId = m.StudentId,
                    StudentName = m.Student.FirstName + " " + m.Student.LastName,
                    Status = m.Status,
                    IsActive = m.IsActive
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müəllim görüşləri yüklənərkən xəta: {TeacherId}", teacherId);
            return new List<MeetingListViewModel>();
        }
    }

    #endregion
}