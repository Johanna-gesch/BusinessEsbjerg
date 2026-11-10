using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infra.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Service.Security;

public class TokenService (IConfiguration configuration) : ITokenService
{
    private const string Issuer = "security-api";
    private const string Audience = "security-api";
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(
        configuration["JwtSecret"] ?? throw new InvalidOperationException("JwtSecret is missing.")));

    public string GenerateToken(User user)
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId),

            new Claim(ClaimTypes.Name, user.Username),

            new Claim("isAdmin", user.IsAdmin.ToString().ToLower())
        ];

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}