using AppointmentSystem.Models.ViewModels;

namespace AppointmentSystem.Services.Abstract;

/// <summary>
/// Company service interface
/// </summary>
public interface ICompanyService
{
    Task<List<CompanyCardViewModel>> GetAllActiveCompaniesAsync();
    Task<CompanyCardViewModel?> GetCompanyByIdAsync(Guid companyId);
    Task<bool> CompanyExistsAsync(Guid companyId);
}
