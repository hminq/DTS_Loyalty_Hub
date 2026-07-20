using System.IdentityModel.Tokens.Jwt;

namespace Api.Authentication;

public sealed class AuthenticatedAdminSessionAccessor : IAuthenticatedAdminSessionAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedAdminSessionAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool TryGet(out AuthenticatedAdminSession session)
    {
        session = default!;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null ||
            !TryReadClaimGuid(user, JwtRegisteredClaimNames.Sub, out var userId) ||
            !TryReadClaimGuid(user, "admin_id", out var adminId) ||
            !TryReadClaimGuid(user, "sid", out var adminSessionId) ||
            !TryReadClaimGuid(user, JwtRegisteredClaimNames.Jti, out var accessTokenJti))
        {
            return false;
        }

        session = new AuthenticatedAdminSession(
            userId,
            adminId,
            adminSessionId,
            accessTokenJti);
        return true;
    }

    private static bool TryReadClaimGuid(
        System.Security.Claims.ClaimsPrincipal user,
        string claimType,
        out Guid value)
    {
        var rawValue = user.FindFirst(claimType)?.Value;

        return Guid.TryParse(rawValue, out value);
    }
}
