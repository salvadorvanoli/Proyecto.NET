using SQLite;

namespace Mobile.Data;

public class LocalDatabase : ILocalDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public async Task InitializeDatabaseAsync()
    {
        if (_database != null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "credenciales.db");
            _database = new SQLiteAsyncConnection(dbPath);
            
            await _database.CreateTableAsync<LocalAccessEvent>();
            
            System.Diagnostics.Debug.WriteLine($"✅ Database initialized at: {dbPath}");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<int> SaveAccessEventAsync(LocalAccessEvent accessEvent)
    {
        await InitializeDatabaseAsync();
        
        if (accessEvent.Id == 0)
        {
            return await _database!.InsertAsync(accessEvent);
        }
        else
        {
            return await _database!.UpdateAsync(accessEvent);
        }
    }

    public async Task<List<LocalAccessEvent>> GetAccessEventsAsync(int userId, int skip = 0, int take = 20)
    {
        await InitializeDatabaseAsync();
        
        return await _database!.Table<LocalAccessEvent>()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<LocalAccessEvent>> GetUnsyncedEventsAsync(int userId)
    {
        await InitializeDatabaseAsync();
        
        return await _database!.Table<LocalAccessEvent>()
            .Where(e => e.UserId == userId && !e.IsSynced)
            .ToListAsync();
    }

    public async Task MarkEventAsSyncedAsync(int eventId)
    {
        await InitializeDatabaseAsync();
        
        var evt = await _database!.FindAsync<LocalAccessEvent>(eventId);
        if (evt != null)
        {
            evt.IsSynced = true;
            await _database.UpdateAsync(evt);
        }
    }

    public async Task<int> GetTotalEventsCountAsync(int userId)
    {
        await InitializeDatabaseAsync();
        
        return await _database!.Table<LocalAccessEvent>()
            .Where(e => e.UserId == userId)
            .CountAsync();
    }
}
