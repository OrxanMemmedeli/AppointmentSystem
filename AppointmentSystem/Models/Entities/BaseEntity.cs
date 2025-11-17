namespace AppointmentSystem.Models.Entities;

/// <summary>
/// ID, status və zaman damğaları olan baza entiti
/// </summary>
public abstract class BaseEntity : Entity
{
    /// <summary>Unikal identifikator</summary>
    public Guid Id { get; set; }

    /// <summary>Aktivlik statusu</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Silinmə (soft delete) işarəsi</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Yaradılma vaxtı (UTC)</summary>
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Dəyişdirilmə vaxtı (UTC)</summary>
    public DateTimeOffset? ModifiedDate { get; set; }
}
