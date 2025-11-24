using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Şagird-Valideyn əlaqəsi idarəetmə servisi implementasiyası
/// </summary>
public class StudentParentService : IStudentParentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentParentService> _logger;

    public StudentParentService(
        AppDbContext context,
        ILogger<StudentParentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün şagird-valideyn əlaqələrini gətirir</summary>
    public async Task<List<StudentParentListViewModel>> GetAllStudentParentsAsync()
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => !sp.IsDeleted)
            .Select(sp => new StudentParentListViewModel
            {
                Id = sp.Id,
                StudentId = sp.StudentId,
                StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                StudentClassName = sp.Student.Class != null ? sp.Student.Class.Name : null,
                ParentId = sp.ParentId,
                ParentName = sp.Parent.FirstName + " " + sp.Parent.LastName,
                ParentFinCode = sp.Parent.FinCode,
                ParentTypeId = sp.ParentTypeId,
                ParentTypeName = sp.ParentType.Name,
                RelationType = sp.RelationType,
                RelationTypeDisplay = sp.RelationType.ToString(),
                IsPrimaryContact = sp.IsPrimaryContact,
                IsActive = sp.IsActive,
                CreatedDate = sp.CreatedDate
            })
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    /// <summary>Şagirdə görə valideynləri gətirir</summary>
    public async Task<List<StudentParentListViewModel>> GetParentsByStudentAsync(Guid studentId)
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => !sp.IsDeleted && sp.StudentId == studentId)
            .Select(sp => new StudentParentListViewModel
            {
                Id = sp.Id,
                StudentId = sp.StudentId,
                StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                StudentClassName = sp.Student.Class != null ? sp.Student.Class.Name : null,
                ParentId = sp.ParentId,
                ParentName = sp.Parent.FirstName + " " + sp.Parent.LastName,
                ParentFinCode = sp.Parent.FinCode,
                ParentTypeId = sp.ParentTypeId,
                ParentTypeName = sp.ParentType.Name,
                RelationType = sp.RelationType,
                RelationTypeDisplay = sp.RelationType.ToString(),
                IsPrimaryContact = sp.IsPrimaryContact,
                IsActive = sp.IsActive,
                CreatedDate = sp.CreatedDate
            })
            .OrderByDescending(sp => sp.IsPrimaryContact)
            .ThenBy(sp => sp.RelationType)
            .ToListAsync();
    }

    /// <summary>Valideyinə görə şagirdləri gətirir</summary>
    public async Task<List<StudentParentListViewModel>> GetStudentsByParentAsync(Guid parentId)
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => !sp.IsDeleted && sp.ParentId == parentId)
            .Select(sp => new StudentParentListViewModel
            {
                Id = sp.Id,
                StudentId = sp.StudentId,
                StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                StudentClassName = sp.Student.Class != null ? sp.Student.Class.Name : null,
                ParentId = sp.ParentId,
                ParentName = sp.Parent.FirstName + " " + sp.Parent.LastName,
                ParentFinCode = sp.Parent.FinCode,
                ParentTypeId = sp.ParentTypeId,
                ParentTypeName = sp.ParentType.Name,
                RelationType = sp.RelationType,
                RelationTypeDisplay = sp.RelationType.ToString(),
                IsPrimaryContact = sp.IsPrimaryContact,
                IsActive = sp.IsActive,
                CreatedDate = sp.CreatedDate
            })
            .OrderBy(sp => sp.StudentName)
            .ToListAsync();
    }

    /// <summary>ID-yə görə əlaqə gətirir</summary>
    public async Task<StudentParentViewModel?> GetStudentParentByIdAsync(Guid id)
    {
        return await _context.StudentParents
            .AsNoTracking()
            .Where(sp => sp.Id == id && !sp.IsDeleted)
            .Select(sp => new StudentParentViewModel
            {
                Id = sp.Id,
                StudentId = sp.StudentId,
                ParentId = sp.ParentId,
                ParentTypeId = sp.ParentTypeId,
                RelationType = sp.RelationType,
                IsPrimaryContact = sp.IsPrimaryContact,
                IsActive = sp.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>Şagird-valideyn əlaqəsinin mövcudluğunu yoxlayır</summary>
    public async Task<bool> StudentParentExistsAsync(Guid id)
    {
        return await _context.StudentParents
            .AnyAsync(sp => sp.Id == id && !sp.IsDeleted);
    }

    /// <summary>Əlaqənin mövcudluğunu yoxlayır (duplicate check)</summary>
    public async Task<bool> RelationshipExistsAsync(Guid studentId, Guid parentId, Guid? excludeId = null)
    {
        var query = _context.StudentParents
            .Where(sp => !sp.IsDeleted &&
                         sp.StudentId == studentId &&
                         sp.ParentId == parentId);

        if (excludeId.HasValue)
        {
            query = query.Where(sp => sp.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>Yeni şagird-valideyn əlaqəsi yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? StudentParentId)> CreateStudentParentAsync(
        StudentParentViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Əlaqənin mövcudluğunu yoxla
            var exists = await RelationshipExistsAsync(model.StudentId, model.ParentId);
            if (exists)
            {
                return (false, "Bu şagird və valideyn artıq əlaqələndirilib", null);
            }

            var studentParent = new StudentParent
            {
                Id = Guid.NewGuid(),
                StudentId = model.StudentId,
                ParentId = model.ParentId,
                ParentTypeId = model.ParentTypeId,
                RelationType = model.RelationType,
                IsPrimaryContact = model.IsPrimaryContact,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedById = currentUserId
            };

            // Əgər bu əsas valideyn olaraq təyin olunubsa, digər əsas valideynləri sıfırla
            if (model.IsPrimaryContact)
            {
                var existingPrimary = await _context.StudentParents
                    .Where(sp => !sp.IsDeleted &&
                                 sp.StudentId == model.StudentId &&
                                 sp.IsPrimaryContact)
                    .ToListAsync();

                foreach (var sp in existingPrimary)
                {
                    sp.IsPrimaryContact = false;
                    sp.ModifiedDate = DateTime.UtcNow;
                    sp.ModifiedById = currentUserId;
                }
            }

            _context.StudentParents.Add(studentParent);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni şagird-valideyn əlaqəsi yaradıldı: StudentId={StudentId}, ParentId={ParentId}",
                studentParent.StudentId, studentParent.ParentId);

            return (true, null, studentParent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Şagird-valideyn əlaqəsi yaradılarkən xəta: StudentId={StudentId}, ParentId={ParentId}",
                model.StudentId, model.ParentId);
            return (false, "Əlaqə yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Şagird-valideyn əlaqəsini yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateStudentParentAsync(
        StudentParentViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Əlaqə ID-si tələb olunur");
            }

            var studentParent = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.Id == model.Id.Value && !sp.IsDeleted);

            if (studentParent == null)
            {
                return (false, "Əlaqə tapılmadı");
            }

            // Əlaqənin mövcudluğunu yoxla (şagird və ya valideyn dəyişdirilibsə)
            if (model.StudentId != studentParent.StudentId || model.ParentId != studentParent.ParentId)
            {
                var exists = await RelationshipExistsAsync(model.StudentId, model.ParentId, studentParent.Id);
                if (exists)
                {
                    return (false, "Bu şagird və valideyn artıq əlaqələndirilib");
                }
            }

            studentParent.StudentId = model.StudentId;
            studentParent.ParentId = model.ParentId;
            studentParent.ParentTypeId = model.ParentTypeId;
            studentParent.RelationType = model.RelationType;
            studentParent.IsPrimaryContact = model.IsPrimaryContact;
            studentParent.IsActive = model.IsActive;
            studentParent.ModifiedDate = DateTime.UtcNow;
            studentParent.ModifiedById = currentUserId;

            // Əgər bu əsas valideyn olaraq təyin olunubsa, digər əsas valideynləri sıfırla
            if (model.IsPrimaryContact)
            {
                var existingPrimary = await _context.StudentParents
                    .Where(sp => !sp.IsDeleted &&
                                 sp.StudentId == model.StudentId &&
                                 sp.IsPrimaryContact &&
                                 sp.Id != studentParent.Id)
                    .ToListAsync();

                foreach (var sp in existingPrimary)
                {
                    sp.IsPrimaryContact = false;
                    sp.ModifiedDate = DateTime.UtcNow;
                    sp.ModifiedById = currentUserId;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Şagird-valideyn əlaqəsi yeniləndi: ID={Id}",
                studentParent.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird-valideyn əlaqəsi yenilənərkən xəta: ID {Id}", model.Id);
            return (false, "Əlaqə yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Əsas valideyni təyin edir</summary>
    public async Task<(bool Success, string? ErrorMessage)> SetPrimaryContactAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var studentParent = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.Id == id && !sp.IsDeleted);

            if (studentParent == null)
            {
                return (false, "Əlaqə tapılmadı");
            }

            // Digər əsas valideynləri sıfırla
            var existingPrimary = await _context.StudentParents
                .Where(sp => !sp.IsDeleted &&
                             sp.StudentId == studentParent.StudentId &&
                             sp.IsPrimaryContact)
                .ToListAsync();

            foreach (var sp in existingPrimary)
            {
                sp.IsPrimaryContact = false;
                sp.ModifiedDate = DateTime.UtcNow;
                sp.ModifiedById = currentUserId;
            }

            // Yeni əsas valideyni təyin et
            studentParent.IsPrimaryContact = true;
            studentParent.ModifiedDate = DateTime.UtcNow;
            studentParent.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Əsas valideyn təyin edildi: StudentParentId={Id}",
                studentParent.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Əsas valideyn təyin edilərkən xəta: ID {Id}", id);
            return (false, "Əsas valideyn təyin edilərkən xəta baş verdi");
        }
    }

    /// <summary>Şagird-valideyn əlaqəsini silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteStudentParentAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var studentParent = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.Id == id && !sp.IsDeleted);

            if (studentParent == null)
            {
                return (false, "Əlaqə tapılmadı");
            }

            studentParent.IsDeleted = true;
            studentParent.IsActive = false;
            studentParent.ModifiedDate = DateTime.UtcNow;
            studentParent.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Şagird-valideyn əlaqəsi silindi: ID={Id}",
                studentParent.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şagird-valideyn əlaqəsi silinərkən xəta: ID {Id}", id);
            return (false, "Əlaqə silinərkən xəta baş verdi");
        }
    }
}