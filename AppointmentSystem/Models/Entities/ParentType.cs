using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Valideyn növü
/// </summary>
public class ParentType : AuditableEntity
{
    /// <summary>Adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Açıqlama</summary>
    public string? Description { get; set; }

    /// <summary>Növ (enum)</summary>
    public ParentRelationType Type { get; set; }

    #region Navigation Properties
    /// <summary>Şagird-Valideyn əlaqələri</summary>
    public virtual ICollection<StudentParent> StudentParents { get; set; } = new HashSet<StudentParent>();
    #endregion
}