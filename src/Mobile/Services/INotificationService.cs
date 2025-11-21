namespace Mobile.Services;

public interface INotificationService
{
    Task ShowNotification(string title, string message);
}
