namespace Mobile.Services;

public interface ISyncService
{
    Task SyncPendingEventsAsync();
    Task<bool> CheckUserStatusAsync();
}
