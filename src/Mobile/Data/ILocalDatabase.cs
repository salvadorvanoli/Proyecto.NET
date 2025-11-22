namespace Mobile.Data;

public interface ILocalDatabase
{
    Task InitializeDatabaseAsync();
    Task<int> SaveAccessEventAsync(LocalAccessEvent accessEvent);
    Task<List<LocalAccessEvent>> GetAccessEventsAsync(int userId, int skip = 0, int take = 20);
    Task<List<LocalAccessEvent>> GetUnsyncedEventsAsync(int userId);
    Task MarkEventAsSyncedAsync(int eventId);
    Task<int> GetTotalEventsCountAsync(int userId);
    Task DeleteAllUserAccessEventsAsync(int userId);
    Task DeleteUnsyncedEventsAsync(int userId);
}
