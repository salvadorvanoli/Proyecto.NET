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
            
            // Forzar recreación de tabla para agregar BackendId
            try
            {
                await _database.DropTableAsync<LocalAccessEvent>();
                System.Diagnostics.Debug.WriteLine("🗑️ Dropped old AccessEvents table");
            }
            catch
            {
                // Si no existe la tabla, no pasa nada
            }
            
            await _database.CreateTableAsync<LocalAccessEvent>();
            
            System.Diagnostics.Debug.WriteLine($"✅ Database initialized at: {dbPath} with BackendId column");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<int> SaveAccessEventAsync(LocalAccessEvent accessEvent)
    {
        await InitializeDatabaseAsync();
        
        try
        {
            // Si tiene BackendId, intentar actualizar un evento existente con ese BackendId
            if (accessEvent.BackendId.HasValue)
            {
                // Primero eliminar cualquier evento con el mismo BackendId
                await _database!.ExecuteAsync(
                    "DELETE FROM AccessEvents WHERE BackendId = ?",
                    accessEvent.BackendId.Value);
            }
            
            // Insertar el nuevo evento (AutoIncrement generará el ID)
            var result = await _database!.InsertAsync(accessEvent);
            
            System.Diagnostics.Debug.WriteLine($"💾 SaveAccessEvent: LocalID={accessEvent.Id}, BackendID={accessEvent.BackendId}, ControlPoint={accessEvent.ControlPointName}, Result={result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error saving event: {ex.Message}");
            throw;
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

    public async Task DeleteAllUserAccessEventsAsync(int userId)
    {
        await InitializeDatabaseAsync();
        
        var deletedCount = await _database!.ExecuteAsync(
            "DELETE FROM AccessEvents WHERE UserId = ?",
            userId);
        
        System.Diagnostics.Debug.WriteLine($"🗑️ Deleted {deletedCount} access events for user {userId}");
    }

    public async Task DeleteUnsyncedEventsAsync(int userId)
    {
        await InitializeDatabaseAsync();
        
        var deletedCount = await _database!.ExecuteAsync(
            "DELETE FROM LocalAccessEvent WHERE UserId = ? AND IsSynced = ?",
            userId, false);
        
        System.Diagnostics.Debug.WriteLine($"🗑️ Deleted {deletedCount} unsynced events for user {userId}");
    }
}
