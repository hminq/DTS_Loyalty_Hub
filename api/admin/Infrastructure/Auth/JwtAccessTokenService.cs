using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

public sealed class JwtAccessTokenService : IAccessTokenService
{
    private readonly JwtOptions _options;

    public JwtAccessTokenService(JwtOptions options)
    {
        _options = options;
    }

    public AccessToken CreateAccessToken(AdminLoginUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("admin_id", user.AdminId.ToString()),
            new("role_id", user.RoleId.ToString()),
            new(ClaimTypes.Role, user.RoleName)
        };

        claims.AddRange(user.PermissionCodes.Select(permissionCode => new Claim("permission", permissionCode)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
