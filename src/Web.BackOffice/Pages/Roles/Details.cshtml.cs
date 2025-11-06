using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Roles;
using Shared.DTOs.Users;
using Web.BackOffice.Services;
using System.Security.Claims;

namespace Web.BackOffice.Pages.Roles;

public class DetailsModel : PageModel
{
    private readonly IRoleApiService _roleApiService;
    private readonly IUserApiService _userApiService;
    private readonly ILogger<DetailsModel> _logger;
    private const int PageSize = 10;

    public DetailsModel(
        IRoleApiService roleApiService,
        IUserApiService userApiService,
        ILogger<DetailsModel> logger)
    {
        _roleApiService = roleApiService;
        _userApiService = userApiService;
        _logger = logger;
    }

    public RoleResponse? Role { get; set; }
    public IEnumerable<UserResponse> AllUsers { get; set; } = Enumerable.Empty<UserResponse>();
    public IEnumerable<UserResponse> DisplayedUsers { get; set; } = Enumerable.Empty<UserResponse>();
    public IEnumerable<UserResponse> UsersWithRole { get; set; } = Enumerable.Empty<UserResponse>();
    public int CurrentUserId { get; set; }

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }

    // Búsqueda
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, int pageNumber = 1)
    {
        try
        {
            Role = await _roleApiService.GetRoleByIdAsync(id);

            if (Role == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el rol con ID {id}.";
                return RedirectToPage("/Roles/Index");
            }

            // Obtener el ID del usuario actual
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out int userId))
            {
                CurrentUserId = userId;
            }

            // Load all users in the tenant
            AllUsers = await _userApiService.GetAllUsersAsync();

            // Aplicar búsqueda si hay término de búsqueda
            var filteredUsers = AllUsers;
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredUsers = AllUsers.Where(u =>
                    u.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Calcular paginación
            TotalUsers = filteredUsers.Count();
            TotalPages = (int)Math.Ceiling(TotalUsers / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener usuarios de la página actual
            DisplayedUsers = filteredUsers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Load users that have this role
            UsersWithRole = await GetUsersWithRoleAsync(id);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role details {RoleId}", id);
            TempData["ErrorMessage"] = "Error al cargar los detalles del rol.";
            return RedirectToPage("/Roles/Index");
        }
    }

    public async Task<IActionResult> OnPostAssignRoleAsync(int roleId, List<int> selectedUserIds)
    {
        try
        {
            // Obtener el ID del usuario actual
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                ErrorMessage = "No se pudo identificar al usuario actual.";
                return RedirectToPage(new { id = roleId });
            }

            // Get all users in the tenant
            var allUsers = await _userApiService.GetAllUsersAsync();
            var allUsersList = allUsers.ToList();

            // For each user, assign or remove the role based on selection
            foreach (var user in allUsersList)
            {
                // Prevenir que el usuario modifique sus propios roles
                if (user.Id == currentUserId)
                {
                    _logger.LogWarning("User {UserId} attempted to modify their own roles", currentUserId);
                    continue; // Saltar al siguiente usuario sin modificar los roles del usuario actual
                }

                var isSelected = selectedUserIds.Contains(user.Id);

                // Get current roles for this user
                var userRoles = await _roleApiService.GetUserRolesAsync(user.Id);
                var userRoleIds = userRoles.Select(r => r.Id).ToList();

                // Determine if we need to update
                var hasRole = userRoleIds.Contains(roleId);

                if (isSelected && !hasRole)
                {
                    // Add role to user
                    userRoleIds.Add(roleId);
                }
                else if (!isSelected && hasRole)
                {
                    // Remove role from user
                    userRoleIds.Remove(roleId);
                }
                else
                {
                    // No change needed for this user
                    continue;
                }

                // Update user's roles
                var assignRoleDto = new AssignRoleRequest
                {
                    RoleIds = userRoleIds
                };

                await _roleApiService.AssignRolesToUserAsync(user.Id, assignRoleDto);
            }

            SuccessMessage = "Asignaciones de roles actualizadas exitosamente.";
            return RedirectToPage(new { id = roleId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to users", roleId);
            ErrorMessage = "Error al asignar el rol a los usuarios.";
            return RedirectToPage(new { id = roleId });
        }
    }

    private async Task<IEnumerable<UserResponse>> GetUsersWithRoleAsync(int roleId)
    {
        try
        {
            var allUsers = await _userApiService.GetAllUsersAsync();
            var usersWithRole = new List<UserResponse>();

            foreach (var user in allUsers)
            {
                var userRoles = await _roleApiService.GetUserRolesAsync(user.Id);
                if (userRoles.Any(r => r.Id == roleId))
                {
                    usersWithRole.Add(user);
                }
            }

            return usersWithRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with role {RoleId}", roleId);
            return Enumerable.Empty<UserResponse>();
        }
    }
}
