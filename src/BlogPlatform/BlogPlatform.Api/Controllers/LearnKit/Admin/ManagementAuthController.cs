using BlogPlatform.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin;

[ApiController]
[Route("api/learnkit/admin/auth")]
public sealed class ManagementAuthController(
    LearnKitManagementTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] ManagementLoginRequest request)
    {
        var token = tokenService.CreateToken(request.Username, request.Password);

        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new ManagementLoginResponse(token));
    }
}

public sealed record ManagementLoginRequest(string? Username, string? Password);
public sealed record ManagementLoginResponse(string AccessToken);
