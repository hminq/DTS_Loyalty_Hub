using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly IAccessTokenService _accessTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordVerifier passwordVerifier,
        IAccessTokenService accessTokenService)
    {
        _userRepository = userRepository;
        _passwordVerifier = passwordVerifier;
        _accessTokenService = accessTokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (user is null || !IsEnabled(user) || !_passwordVerifier.Verify(user.PasswordHash, request.Password))
        {
            throw new DomainException(
                "INVALID_CREDENTIALS",
                DomainErrorType.Unauthorized);
        }

        var expiresAt = _accessTokenService.CreateExpiresAt();
        var accessToken = _accessTokenService.CreateAccessToken(user.ToTokenUser(), expiresAt);

        return new LoginResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            new CustomerLoginResult(
                user.UserId,
                user.CustomerId,
                user.Username,
                user.FullName));
    }

    private static bool IsEnabled(CustomerLoginUser user)
    {
        return UserStatus.IsEnabled(user.Status);
    }
}
