namespace Backend55.Portal.Services;

/// <summary>
/// Provides the development user selected for the current Blazor session.
/// </summary>
public sealed class DevelopmentUserState
{
    /// <summary>
    /// Name of the cookie storing the selected development user.
    /// </summary>
    public const string CookieName = "Backend55.DevelopmentUser";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DevelopmentUserState"/> class.
    /// </summary>
    public DevelopmentUserState(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        Current = ReadInitialUser();
    }

    /// <summary>
    /// Currently selected development user.
    /// </summary>
    public DevelopmentUser Current { get; }

    private DevelopmentUser ReadInitialUser()
    {
        var cookieValue = _httpContextAccessor
            .HttpContext?
            .Request
            .Cookies[CookieName];

        if (!Guid.TryParse(cookieValue, out var userId))
        {
            return DevelopmentUsers.Default;
        }

        return DevelopmentUsers.All.FirstOrDefault(
                   user => user.Id == userId)
               ?? DevelopmentUsers.Default;
    }
}
