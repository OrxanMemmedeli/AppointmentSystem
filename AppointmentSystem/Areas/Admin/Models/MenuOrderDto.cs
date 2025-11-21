namespace AppointmentSystem.Areas.Admin.Models;

/// <summary>
/// Menyu sıralama DTO
/// </summary>
public class MenuOrderDto
{
    public Guid Id { get; set; }
    public int OrderIndex { get; set; }
}