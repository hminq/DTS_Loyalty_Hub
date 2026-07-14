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

    public DateTime CreateExpiresAt()
    {
        return DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
    }

    public AccessToken CreateAccessToken(AdminLoginUser user, AdminLoginSession session)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, session.AccessTokenJti.ToString()),
            new("sid", session.AdminSessionId.ToString()),
            new("admin_id", user.AdminId.ToString()),
            new("role_id", user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: session.ExpiresAt,
            signingCredentials: signingCredentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            session.ExpiresAt);
    }
}
