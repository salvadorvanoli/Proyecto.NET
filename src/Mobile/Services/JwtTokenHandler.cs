namespace Mobile.Services;

/// <summary>
/// DelegatingHandler que agrega el JWT token a todas las peticiones HTTP
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public JwtTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.Token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
                System.Diagnostics.Debug.WriteLine($"[JwtTokenHandler] Added JWT token to request: {request.RequestUri}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[JwtTokenHandler] No token available for request: {request.RequestUri}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JwtTokenHandler] Error adding JWT token: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
