namespace AppointmentSystem.Models.Enums;

/// <summary>
/// Görüş statusları
/// </summary>
public enum MeetingStatus
{
    /// <summary>Pending - gözləmədə</summary>
    Pending = 1,
    
    /// <summary>Approved - təsdiqlənmiş</summary>
    Approved = 2,
    
    /// <summary>Declined - rədd edilmiş</summary>
    Declined = 3,
    
    /// <summary>Cancelled - ləğv edilmiş</summary>
    Cancelled = 4,
    
    /// <summary>Completed - tamamlanmış</summary>
    Completed = 5
}
