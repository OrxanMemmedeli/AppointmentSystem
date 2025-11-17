using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

/// <summary>
/// Sinif
/// </summary>
public class SchoolClass : AuditableEntity
{
    /// <summary>Sinif adı (məs: 10-A, 11-B)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sinif səviyyəsi (1-11)</summary>
    public int Level { get; set; }

    /// <summary>Şöbə (A, B, C)</summary>
    public string? Section { get; set; }

    /// <summary>Açıqlama</summary>
    public string? Description { get; set; }

    /// <summary>Şirkət ID</summary>
    public Guid CompanyId { get; set; }

    #region Navigation Properties
    /// <summary>Şirkət</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Sinif şagirdləri</summary>
    public virtual ICollection<Student> Students { get; set; } = new HashSet<Student>();

    /// <summary>Sinif müəllimləri</summary>
    public virtual ICollection<ClassTeacher> ClassTeachers { get; set; } = new HashSet<ClassTeacher>();

    /// <summary>Müəllim-Sinif əlaqələri</summary>
    public virtual ICollection<TeacherClass> TeacherClasses { get; set; } = new HashSet<TeacherClass>();
    #endregion
}