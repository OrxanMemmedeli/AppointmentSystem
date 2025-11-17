using AppointmentSystem.Models.Entities;

namespace AppointmentSystem.Models.Entities;

public class UserType : AuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }

    #region Navigation Properties
    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
    #endregion
}
