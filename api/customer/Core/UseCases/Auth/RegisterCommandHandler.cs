using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly IAccessTokenService _accessTokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordVerifier passwordVerifier,
        IAccessTokenService accessTokenService)
    {
        _userRepository = userRepository;
        _passwordVerifier = passwordVerifier;
        _accessTokenService = accessTokenService;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var email = UserProfileRules.NormalizeEmail(request.Email);
        var fullName = UserProfileRules.NormalizeFullName(request.FullName);
        var phone = UserProfileRules.NormalizePhoneNumber(request.Phone);

        if (await _userRepository.ExistsByUsernameAsync(username, ct))
        {
            throw new DomainException(
                "USERNAME_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        if (await _userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        if (await _userRepository.ExistsByPhoneAsync(phone, ct))
        {
            throw new DomainException(
                "PHONE_ALREADY_EXISTS",
                DomainErrorType.Conflict);
        }

        var passwordHash = _passwordVerifier.Hash(request.Password);

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var created = _userRepository.Add(
            userId,
            customerId,
            new NewCustomerUser(
                username,
                email,
                passwordHash,
                fullName,
                phone));

        var expiresAt = _accessTokenService.CreateExpiresAt();

        var accessToken = _accessTokenService.CreateAccessToken(
            new CustomerTokenUser(
                created.UserId,
                created.CustomerId,
                username),
            expiresAt);

        return new RegisterResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            new CustomerRegisterResult(
                created.UserId,
                created.CustomerId,
                username,
                email,
                fullName));
    }
}
