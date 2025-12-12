using System.Security.Claims;

namespace TheKittySaver.AdoptionSystem.API.Authorization;

/// <summary>
/// Service for accessing the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the claims principal of the current user.
    /// </summary>
    ClaimsPrincipal? User { get; }

    /// <summary>
    /// Checks if the current user is in the specified role.
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Gets all roles assigned to the current user.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets a specific claim value by type.
    /// </summary>
    string? GetClaim(string claimType);

    /// <summary>
    /// Gets all claims of the current user.
    /// </summary>
    IEnumerable<Claim> Claims { get; }
}

/// <summary>
/// Implementation of ICurrentUserService using HttpContext.
/// </summary>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub");

    public string? UserName => GetClaim(ClaimTypes.Name) ?? GetClaim("name");

    public string? Email => GetClaim(ClaimTypes.Email) ?? GetClaim("email");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public IEnumerable<Claim> Claims => User?.Claims ?? Enumerable.Empty<Claim>();

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    public string? GetClaim(string claimType) => User?.FindFirst(claimType)?.Value;
}
