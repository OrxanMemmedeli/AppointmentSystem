namespace AppointmentSystem.Models.ViewModels;

public class CompanyViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public string? BackgroundImagePath { get; set; }
    public string? MapUrl { get; set; }
    public string? Description { get; set; }
}
