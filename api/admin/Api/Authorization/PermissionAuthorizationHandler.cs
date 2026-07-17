using System.IdentityModel.Tokens.Jwt;
using Api.Authentication;
using Core.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAdminSessionRepository _adminSessionRepository;
    private readonly IAdminPermissionChecker _adminPermissionChecker;
    private readonly CurrentAdminContext _currentAdminContext;

    public PermissionAuthorizationHandler(
        IAdminSessionRepository adminSessionRepository,
        IAdminPermissionChecker adminPermissionChecker,
        CurrentAdminContext currentAdminContext)
    {
        _adminSessionRepository = adminSessionRepository;
        _adminPermissionChecker = adminPermissionChecker;
        _currentAdminContext = currentAdminContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!TryReadClaimGuid(context, JwtRegisteredClaimNames.Sub, out var userId) ||
            !TryReadClaimGuid(context, "admin_id", out var adminId) ||
            !TryReadClaimGuid(context, "sid", out var adminSessionId) ||
            !TryReadClaimGuid(context, JwtRegisteredClaimNames.Jti, out var accessTokenJti))
        {
            return;
        }

        var isSessionActive = await _adminSessionRepository.IsSessionActiveAsync(
            adminSessionId,
            accessTokenJti,
            adminId,
            userId);

        if (!isSessionActive)
        {
            return;
        }

        var hasPermission = await _adminPermissionChecker.HasPermissionAsync(
            adminId,
            requirement.PermissionCode);

        if (hasPermission)
        {
            _currentAdminContext.Set(
                userId,
                adminId,
                adminSessionId,
                accessTokenJti);

            context.Succeed(requirement);
        }
    }

    private static bool TryReadClaimGuid(
        AuthorizationHandlerContext context,
        string claimType,
        out Guid value)
    {
        var rawValue = context.User.FindFirst(claimType)?.Value;

        return Guid.TryParse(rawValue, out value);
    }
}
