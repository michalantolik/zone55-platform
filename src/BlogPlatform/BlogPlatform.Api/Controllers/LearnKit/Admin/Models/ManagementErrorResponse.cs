namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Stable error returned by LearnKit management endpoints.
/// </summary>
/// <param name="Code">Machine-readable error code.</param>
/// <param name="Message">User-facing explanation of the failure.</param>
public sealed record ManagementErrorResponse(
    string Code,
    string Message);
