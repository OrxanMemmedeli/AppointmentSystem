namespace AppointmentSystem.Models.Enums;

/// <summary>
/// Valideyn qohumluq növü
/// </summary>
public enum ParentRelationType
{
    /// <summary>Ata</summary>
    Father = 1,
    
    /// <summary>Ana</summary>
    Mother = 2,
    
    /// <summary>Baba</summary>
    Grandfather = 3,
    
    /// <summary>Nənə</summary>
    Grandmother = 4,
    
    /// <summary>Qardaş</summary>
    Brother = 5,
    
    /// <summary>Bacı</summary>
    Sister = 6,
    
    /// <summary>Digər</summary>
    Other = 99
}
