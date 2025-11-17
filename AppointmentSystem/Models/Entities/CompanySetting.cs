using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Şirkət parametrləri (tarix əsaslı override)
/// </summary>
public class CompanySetting : AuditableEntity
{
    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Parametr adı</summary>
    public string SettingName { get; set; } = string.Empty;

    /// <summary>Parametr dəyəri (JSON)</summary>
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>Başlanğıc tarixi (bu tarixdən etibarən aktiv)</summary>
    public DateOnly? EffectiveFrom { get; set; }

    /// <summary>Bitmə tarixi (bu tarixədək aktiv)</summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>Tətbiq tarixi</summary>
    public DateOnly? EffectiveDate { get; set; }

    /// <summary>Həftə günü (xüsusi gün üçün)</summary>
    public WeekDay? WeekDay { get; set; }

    /// <summary>İş günləri (JSON array)</summary>
    public string? WorkingDays { get; set; }

    /// <summary>İstisna tarixlər (bayram, tətil - JSON array)</summary>
    public string? ExcludedDates { get; set; }

    /// <summary>İstisna vaxt slotları</summary>
    public string? ExcludedTimeSlots { get; set; }

    /// <summary>Qeydlər</summary>
    public string? Notes { get; set; }

    #region Navigation Properties
    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;
    #endregion
}