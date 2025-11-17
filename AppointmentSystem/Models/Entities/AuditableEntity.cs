namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Audit tələb edən entitilər üçün baza: yaradan və dəyişən istifadəçi
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>Yaradan istifadəçi Id-si</summary>
    public Guid? CreatedById { get; set; }

    /// <summary>Dəyişən istifadəçi Id-si</summary>
    public Guid? ModifiedById { get; set; }
}
