using Core.Abstractions;
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

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordVerifier passwordVerifier)
    {
        _userRepository = userRepository;
        _passwordVerifier = passwordVerifier;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _userRepository.ExistsByUsernameAsync(request.Username, ct))
        {
            throw new DomainException(
                "USERNAME_ALREADY_EXISTS",
                "Username is already taken.",
                DomainErrorType.Conflict);
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
        {
            throw new DomainException(
                "EMAIL_ALREADY_EXISTS",
                "Email is already registered.",
                DomainErrorType.Conflict);
        }

        if (await _userRepository.ExistsByPhoneAsync(request.Phone, ct))
        {
            throw new DomainException(
                "PHONE_ALREADY_EXISTS",
                "Phone number is already registered.",
                DomainErrorType.Conflict);
        }

        var passwordHash = _passwordVerifier.Hash(request.Password);

        var created = await _userRepository.CreateAsync(
            new NewCustomerUser(
                request.Username,
                request.Email,
                passwordHash,
                request.FullName,
                request.Phone),
            ct);

        return new RegisterResult(
            created.UserId,
            created.CustomerId,
            request.Username,
            request.Email,
            request.FullName);
    }
}
