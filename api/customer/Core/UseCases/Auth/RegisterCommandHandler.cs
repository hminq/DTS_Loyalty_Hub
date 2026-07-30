using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Auth.Commands;
using Core.UseCases.Auth.Models;
using Core.UseCases.Auth.Results;
using Core.UseCases.Events.Models;
using MediatR;
using Messaging.Contracts.Events;

namespace Core.UseCases.Auth;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IPublishedEventVersionRepository _publishedEventVersionRepository;
    private readonly TimeProvider _timeProvider;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordVerifier passwordVerifier,
        IAccessTokenService accessTokenService,
        IOutboxWriter outboxWriter,
        IPublishedEventVersionRepository publishedEventVersionRepository,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _passwordVerifier = passwordVerifier;
        _accessTokenService = accessTokenService;
        _outboxWriter = outboxWriter;
        _publishedEventVersionRepository = publishedEventVersionRepository;
        _timeProvider = timeProvider;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var email = UserProfileRules.NormalizeEmail(request.Email);
        var fullName = UserProfileRules.NormalizeFullName(request.FullName);
        var phone = UserProfileRules.NormalizePhoneNumber(request.Phone);
        var referralUsername = NormalizeOptionalUsername(request.ReferralUsername);

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

        ReferralCustomer? referrer = null;
        if (referralUsername is not null)
        {
            if (string.Equals(username, referralUsername, StringComparison.Ordinal))
            {
                throw InvalidReferral();
            }

            referrer = await _userRepository.GetReferralCustomerByUsernameAsync(
                referralUsername,
                ct);

            if (referrer is null || !UserStatus.IsEnabled(referrer.Status))
            {
                throw InvalidReferral();
            }
        }

        var accountEventReference = await GetRequiredPublishedEventVersionAsync(
            CustomerPublishedEventVersions.CustomerAccountRegistered.EventType,
            CustomerPublishedEventVersions.CustomerAccountRegistered.EventVersion,
            ct);

        PublishedEventVersionReference? referralEventReference = null;
        if (referrer is not null)
        {
            referralEventReference = await GetRequiredPublishedEventVersionAsync(
                CustomerPublishedEventVersions.CustomerReferralSucceeded.EventType,
                CustomerPublishedEventVersions.CustomerReferralSucceeded.EventVersion,
                ct);
        }

        var passwordHash = _passwordVerifier.Hash(request.Password);

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var occurredAt = _timeProvider.GetUtcNow().UtcDateTime;
        var created = _userRepository.Add(
            userId,
            customerId,
            new NewCustomerUser(
                username,
                email,
                passwordHash,
                fullName,
                phone));

        _outboxWriter.Add(
            new VersionedOutboxEvent<CustomerAccountRegisteredPayload>(
                accountEventReference,
                new EventEnvelope<CustomerAccountRegisteredPayload>(
                    Guid.NewGuid(),
                    accountEventReference.EventType,
                    accountEventReference.EventVersion,
                    occurredAt,
                    new CustomerAccountRegisteredPayload(
                        created.UserId,
                        created.CustomerId,
                        username,
                        fullName))));

        if (referrer is not null && referralEventReference is not null)
        {
            _outboxWriter.Add(
                new VersionedOutboxEvent<CustomerReferralSucceededPayload>(
                    referralEventReference,
                    new EventEnvelope<CustomerReferralSucceededPayload>(
                        Guid.NewGuid(),
                        referralEventReference.EventType,
                        referralEventReference.EventVersion,
                        occurredAt,
                        new CustomerReferralSucceededPayload(
                            referrer.CustomerId,
                            created.CustomerId,
                            username))));
        }

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

    private static string? NormalizeOptionalUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username)
            ? null
            : username.Trim();
    }

    private static DomainException InvalidReferral()
    {
        return new DomainException(
            "REFERRAL_USERNAME_INVALID",
            DomainErrorType.Validation);
    }

    private async Task<PublishedEventVersionReference> GetRequiredPublishedEventVersionAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken)
    {
        var reference = await _publishedEventVersionRepository.GetPublishedAsync(
            eventType,
            eventVersion,
            cancellationToken);

        return reference ?? throw new EventPublicationConfigurationException(eventType, eventVersion);
    }
}
