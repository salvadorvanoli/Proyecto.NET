using Microsoft.JSInterop;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services;

/// <summary>
/// Service to manage tenant theme colors across the application.
/// </summary>
public class TenantThemeService
{
    private readonly ITenantApiService _tenantApiService;
    private readonly IJSRuntime _jsRuntime;
    private TenantThemeDto? _currentTheme;

    public TenantThemeService(ITenantApiService tenantApiService, IJSRuntime jsRuntime)
    {
        _tenantApiService = tenantApiService;
        _jsRuntime = jsRuntime;
    }

    public TenantThemeDto? CurrentTheme => _currentTheme;

    public event Action? OnThemeChanged;

    public async Task LoadThemeAsync(int tenantId)
    {
        _currentTheme = await _tenantApiService.GetTenantThemeAsync(tenantId);
        
        if (_currentTheme != null)
        {
            // Aplicar colores usando CSS variables y JS directo
            await _jsRuntime.InvokeVoidAsync("applyTenantTheme", 
                _currentTheme.PrimaryColor, 
                _currentTheme.SecondaryColor, 
                _currentTheme.AccentColor);
            
            // Esperar un poco y aplicar de nuevo para asegurar
            await Task.Delay(100);
            await _jsRuntime.InvokeVoidAsync("applyTenantTheme", 
                _currentTheme.PrimaryColor, 
                _currentTheme.SecondaryColor, 
                _currentTheme.AccentColor);
            
            OnThemeChanged?.Invoke();
        }
    }

    public string GetPrimaryColor() => _currentTheme?.PrimaryColor ?? "#0A3D62";
    public string GetSecondaryColor() => _currentTheme?.SecondaryColor ?? "#1976D2";
    public string GetAccentColor() => _currentTheme?.AccentColor ?? "#F4C10F";
    public string GetTenantName() => _currentTheme?.Name ?? "Universidad";
}
