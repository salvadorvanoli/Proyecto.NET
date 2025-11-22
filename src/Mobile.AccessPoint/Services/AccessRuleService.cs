using Microsoft.Extensions.Logging;
using Mobile.AccessPoint.Models;
using Shared.DTOs;
using Shared.DTOs.AccessEvents;

namespace Mobile.AccessPoint.Services;

/// <summary>
/// Service for managing access rules - Online validation only
/// AccessPoint siempre está online y valida contra el backend
/// </summary>
public class AccessRuleService
{
    private readonly AccessRuleApiService _apiService;
    private readonly ILogger<AccessRuleService> _logger;

    public AccessRuleService(
        AccessRuleApiService apiService,
        ILogger<AccessRuleService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // No se requiere sincronización - AccessPoint siempre valida online

    // No se requiere validación offline - AccessPoint siempre valida online contra el backend

    // AccessPoint no requiere cache local - todas las operaciones son online
}

