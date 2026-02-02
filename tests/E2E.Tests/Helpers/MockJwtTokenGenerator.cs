using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EShop.E2E.Tests.Helpers;

public static class MockJwtTokenGenerator
{
    public const string TestSecretKey = "E2E-Test-Secret-Key-At-Least-32-Characters-Long!";
    public const string TestIssuer = "https://test-issuer.local";
    public const string TestAudience = "api://eshop-api";

    public static string GenerateToken(
        string userId,
        string[]? roles = null,
        TimeSpan? expiry = null,
        IDictionary<string, string>? additionalClaims = null
    )
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("preferred_username", $"{userId}@test.local"),
        };

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        if (additionalClaims is not null)
        {
            foreach (var claim in additionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateExpiredToken(string userId)
    {
        return GenerateToken(userId, expiry: TimeSpan.FromSeconds(-60));
    }

    public static string GenerateInvalidSignatureToken(string userId)
    {
        var securityKey = new SymmetricSecurityKey(
            "Different-Secret-Key-That-Is-At-Least-32-Chars!"u8.ToArray()
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
