using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlogPlatform.Api.Authentication;

public sealed class LearnKitManagementTokenService(
    IOptions<LearnKitManagementAuthOptions> options)
{
    private readonly LearnKitManagementAuthOptions _options = options.Value;

    public string? CreateToken(string? username, string? password)
    {
        if (!CredentialsAreValid(username, password))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(ClaimTypes.Name, _options.Username),
                new Claim(ClaimTypes.Role, "LearnKitManager")
            ],
            notBefore: now,
            expires: now.AddMinutes(_options.TokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool CredentialsAreValid(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrEmpty(password)
            || !string.Equals(username, _options.Username, StringComparison.Ordinal))
        {
            return false;
        }

        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var configuredHash = Convert.FromHexString(_options.PasswordSha256);

        return CryptographicOperations.FixedTimeEquals(suppliedHash, configuredHash);
    }
}
