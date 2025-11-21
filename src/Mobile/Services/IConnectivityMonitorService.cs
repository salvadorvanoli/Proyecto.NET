namespace Mobile.Services;

public interface IConnectivityMonitorService
{
    void StartMonitoring();
    void StopMonitoring();
}
