using Core.Entities.Constants;
using Core.Exceptions;
using Core.Abstractions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth.Handlers;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAdminSessionRepository _adminSessionRepository;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly IAccessTokenService _accessTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IAdminSessionRepository adminSessionRepository,
        IPasswordVerifier passwordVerifier,
        IAccessTokenService accessTokenService)
    {
        _userRepository = userRepository;
        _adminSessionRepository = adminSessionRepository;
        _passwordVerifier = passwordVerifier;
        _accessTokenService = accessTokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(
            request.Username,
            ct);

        if (user is null || !IsEnabled(user) || !_passwordVerifier.Verify(user.PasswordHash, request.Password))
        {
            throw new DomainException(
                "INVALID_CREDENTIALS",
                DomainErrorType.Unauthorized);
        }

        var expiresAt = _accessTokenService.CreateExpiresAt();
        var session = await _adminSessionRepository.CreateSessionIfNoneActiveAsync(
            user.AdminId,
            user.UserId,
            expiresAt,
            ct);

        if (session is null)
        {
            throw new DomainException(
                "SESSION_ALREADY_ACTIVE",
                DomainErrorType.Conflict);
        }

        var accessToken = _accessTokenService.CreateAccessToken(user, session);

        return new LoginResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            new AdminLoginResult(
                user.UserId,
                user.AdminId,
                user.Username,
                user.FullName,
                user.RoleId,
                user.RoleName),
            user.PermissionCodes);
    }

    private static bool IsEnabled(AdminLoginUser user)
    {
        return UserStatus.IsEnabled(user.Status);
    }
}
