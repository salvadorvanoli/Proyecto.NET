namespace Mobile.Services;

/// <summary>
/// Service for handling navigation in MVVM pattern.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified route.
    /// </summary>
    Task NavigateToAsync(string route);

    /// <summary>
    /// Navigates back.
    /// </summary>
    Task GoBackAsync();
}
