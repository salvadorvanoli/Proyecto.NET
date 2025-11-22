using SQLite;

namespace Mobile.Data;

[Table("AccessEvents")]
public class LocalAccessEvent
{
    [PrimaryKey]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public int ControlPointId { get; set; }
    public string ControlPointName { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool WasGranted { get; set; }
    public string? DenialReason { get; set; }
    
    /// <summary>
    /// Indica si este evento ya fue sincronizado con el servidor
    /// </summary>
    public bool IsSynced { get; set; }
}
