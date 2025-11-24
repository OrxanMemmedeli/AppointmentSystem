using AppointmentSystem.Areas.Admin.Models.ViewModels;
using AppointmentSystem.Data;
using AppointmentSystem.Models.Entities;
using AppointmentSystem.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Concrete;

/// <summary>
/// Valideyn növü idarəetmə servisi implementasiyası
/// </summary>
public class ParentTypeService : IParentTypeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ParentTypeService> _logger;

    public ParentTypeService(
        AppDbContext context,
        ILogger<ParentTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Bütün valideyn növlərini gətirir</summary>
    public async Task<List<ParentTypeListViewModel>> GetAllParentTypesAsync()
    {
        return await _context.ParentTypes
            .AsNoTracking()
            .Where(pt => !pt.IsDeleted)
            .Select(pt => new ParentTypeListViewModel
            {
                Id = pt.Id,
                Name = pt.Name,
                Description = pt.Description,
                Type = pt.Type,
                TypeDisplay = pt.Type.ToString(),
                IsActive = pt.IsActive,
                UsageCount = pt.StudentParents.Count(sp => !sp.IsDeleted),
                CreatedDate = pt.CreatedDate
            })
            .OrderBy(pt => pt.Type)
            .ToListAsync();
    }

    /// <summary>Aktiv valideyn növlərini gətirir</summary>
    public async Task<List<ParentTypeListViewModel>> GetActiveParentTypesAsync()
    {
        return await _context.ParentTypes
            .AsNoTracking()
            .Where(pt => !pt.IsDeleted && pt.IsActive)
            .Select(pt => new ParentTypeListViewModel
            {
                Id = pt.Id,
                Name = pt.Name,
                Type = pt.Type,
                TypeDisplay = pt.Type.ToString(),
                IsActive = pt.IsActive,
                CreatedDate = pt.CreatedDate
            })
            .OrderBy(pt => pt.Type)
            .ToListAsync();
    }

    /// <summary>ID-yə görə valideyn növü gətirir</summary>
    public async Task<ParentTypeViewModel?> GetParentTypeByIdAsync(Guid id)
    {
        return await _context.ParentTypes
            .AsNoTracking()
            .Where(pt => pt.Id == id && !pt.IsDeleted)
            .Select(pt => new ParentTypeViewModel
            {
                Id = pt.Id,
                Name = pt.Name,
                Description = pt.Description,
                Type = pt.Type,
                IsActive = pt.IsActive
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>Valideyn növünün mövcudluğunu yoxlayır</summary>
    public async Task<bool> ParentTypeExistsAsync(Guid id)
    {
        return await _context.ParentTypes
            .AnyAsync(pt => pt.Id == id && !pt.IsDeleted);
    }

    /// <summary>Yeni valideyn növü yaradır</summary>
    public async Task<(bool Success, string? ErrorMessage, Guid? ParentTypeId)> CreateParentTypeAsync(
        ParentTypeViewModel model,
        Guid currentUserId)
    {
        try
        {
            // Ad unikallığını yoxla
            var isUnique = await IsNameUniqueAsync(model.Name);
            if (!isUnique)
            {
                return (false, "Bu adda valideyn növü artıq mövcuddur", null);
            }

            var parentType = new ParentType
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Type = model.Type,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedById = currentUserId
            };

            _context.ParentTypes.Add(parentType);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Yeni valideyn növü yaradıldı: {ParentTypeName} (ID: {ParentTypeId})",
                parentType.Name, parentType.Id);

            return (true, null, parentType.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn növü yaradılarkən xəta: {ParentTypeName}", model.Name);
            return (false, "Valideyn növü yaradılarkən xəta baş verdi", null);
        }
    }

    /// <summary>Valideyn növünü yeniləyir</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateParentTypeAsync(
        ParentTypeViewModel model,
        Guid currentUserId)
    {
        try
        {
            if (!model.Id.HasValue)
            {
                return (false, "Valideyn növü ID-si tələb olunur");
            }

            var parentType = await _context.ParentTypes
                .FirstOrDefaultAsync(pt => pt.Id == model.Id.Value && !pt.IsDeleted);

            if (parentType == null)
            {
                return (false, "Valideyn növü tapılmadı");
            }

            // Ad unikallığını yoxla
            if (model.Name != parentType.Name)
            {
                var isUnique = await IsNameUniqueAsync(model.Name, parentType.Id);
                if (!isUnique)
                {
                    return (false, "Bu adda valideyn növü artıq mövcuddur");
                }
            }

            parentType.Name = model.Name.Trim();
            parentType.Description = model.Description?.Trim();
            parentType.Type = model.Type;
            parentType.IsActive = model.IsActive;
            parentType.ModifiedDate = DateTime.UtcNow;
            parentType.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Valideyn növü yeniləndi: {ParentTypeName} (ID: {ParentTypeId})",
                parentType.Name, parentType.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn növü yenilənərkən xəta: ID {ParentTypeId}", model.Id);
            return (false, "Valideyn növü yenilənərkən xəta baş verdi");
        }
    }

    /// <summary>Valideyn növü statusunu dəyişir</summary>
    public async Task<(bool Success, string? ErrorMessage)> ToggleParentTypeStatusAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var parentType = await _context.ParentTypes
                .FirstOrDefaultAsync(pt => pt.Id == id && !pt.IsDeleted);

            if (parentType == null)
            {
                return (false, "Valideyn növü tapılmadı");
            }

            parentType.IsActive = !parentType.IsActive;
            parentType.ModifiedDate = DateTime.UtcNow;
            parentType.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Valideyn növü statusu dəyişdi: {ParentTypeName} - Yeni status: {IsActive}",
                parentType.Name, parentType.IsActive);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn növü statusu dəyişərkən xəta: ID {ParentTypeId}", id);
            return (false, "Status dəyişərkən xəta baş verdi");
        }
    }

    /// <summary>Valideyn növünü silir</summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteParentTypeAsync(
        Guid id,
        Guid currentUserId)
    {
        try
        {
            var parentType = await _context.ParentTypes
                .Include(pt => pt.StudentParents)
                .FirstOrDefaultAsync(pt => pt.Id == id && !pt.IsDeleted);

            if (parentType == null)
            {
                return (false, "Valideyn növü tapılmadı");
            }

            // İstifadə yoxla
            var usageCount = parentType.StudentParents.Count(sp => !sp.IsDeleted);
            if (usageCount > 0)
            {
                return (false,
                    $"Bu valideyn növü {usageCount} dəfə istifadə olunur. " +
                    "Əvvəlcə bağlı olan əlaqələri silin.");
            }

            parentType.IsDeleted = true;
            parentType.IsActive = false;
            parentType.ModifiedDate = DateTime.UtcNow;
            parentType.ModifiedById = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Valideyn növü silindi: {ParentTypeName} (ID: {ParentTypeId})",
                parentType.Name, parentType.Id);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valideyn növü silinərkən xəta: ID {ParentTypeId}", id);
            return (false, "Valideyn növü silinərkən xəta baş verdi");
        }
    }

    /// <summary>Ad unikallığını yoxlayır</summary>
    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = _context.ParentTypes
            .Where(pt => !pt.IsDeleted && pt.Name == normalizedName);

        if (excludeId.HasValue)
        {
            query = query.Where(pt => pt.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>Valideyn növü select list gətirir</summary>
    public async Task<List<SelectListItem>> GetParentTypeSelectListAsync()
    {
        return await _context.ParentTypes
            .AsNoTracking()
            .Where(pt => !pt.IsDeleted && pt.IsActive)
            .OrderBy(pt => pt.Type)
            .Select(pt => new SelectListItem
            {
                Value = pt.Id.ToString(),
                Text = pt.Name
            })
            .ToListAsync();
    }
}
