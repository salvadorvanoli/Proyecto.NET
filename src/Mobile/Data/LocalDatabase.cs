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
        
        // InsertOrReplace funciona con la PrimaryKey (Id)
        // Si existe, actualiza. Si no existe, inserta.
        return await _database!.InsertOrReplaceAsync(accessEvent);
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

    public async Task DeleteAllUserAccessEventsAsync(int userId)
    {
        await InitializeDatabaseAsync();
        
        await _database!.ExecuteAsync(
            "DELETE FROM LocalAccessEvent WHERE UserId = ?",
            userId);
        
        System.Diagnostics.Debug.WriteLine($"🗑️ Deleted all access events for user {userId}");
    }
}
