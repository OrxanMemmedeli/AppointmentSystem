namespace AppointmentSystem.Models.ViewModels;

/// <summary>
/// Şirkət seçim kartı üçün ViewModel
/// </summary>
public class CompanyCardViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public string? BackgroundImagePath { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? MapCoordinates { get; set; }
    public string? MapUrl { get; set; }
}
