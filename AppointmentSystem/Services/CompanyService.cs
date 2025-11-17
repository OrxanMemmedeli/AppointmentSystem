using AppointmentSystem.Data;
using AppointmentSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services;

/// <summary>
/// Company service interface
/// </summary>
public interface ICompanyService
{
    Task<List<CompanyCardViewModel>> GetAllActiveCompaniesAsync();
    Task<CompanyCardViewModel?> GetCompanyByIdAsync(Guid companyId);
    Task<bool> CompanyExistsAsync(Guid companyId);
}

/// <summary>
/// Company service implementation
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyCardViewModel>> GetAllActiveCompaniesAsync()
    {
        return await _context.Companies
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name)
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
                MapCoordinates = c.MapCoordinates
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CompanyCardViewModel?> GetCompanyByIdAsync(Guid companyId)
    {
        return await _context.Companies
            .Where(c => c.Id == companyId && c.IsActive && !c.IsDeleted)
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
                MapCoordinates = c.MapCoordinates
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CompanyExistsAsync(Guid companyId)
    {
        return await _context.Companies
            .AnyAsync(c => c.Id == companyId && c.IsActive && !c.IsDeleted);
    }
}
