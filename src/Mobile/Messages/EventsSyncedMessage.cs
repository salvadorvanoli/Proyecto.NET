namespace Mobile.Messages;

/// <summary>
/// Mensaje enviado cuando se completa la sincronización de eventos
/// </summary>
public class EventsSyncedMessage
{
    public int SyncedCount { get; set; }
}
