using Mobile.Models;

namespace Mobile.Services;

public interface IAccessEventService
{
    Task<List<AccessEventDto>> GetMyAccessEventsAsync(int skip = 0, int take = 20);
    Task<int> GetTotalAccessEventsCountAsync();
    Task<bool> SaveAccessEventAsync(AccessEventDto accessEvent);
}
