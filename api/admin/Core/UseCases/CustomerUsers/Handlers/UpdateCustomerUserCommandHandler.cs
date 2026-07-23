using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.CustomerUsers.Commands;
using Core.UseCases.CustomerUsers.Results;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class UpdateCustomerUserCommandHandler
    : IRequestHandler<UpdateCustomerUserCommand, CustomerUserDetailResult>
{
    private readonly ICustomerUserRepository _customerUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateCustomerUserCommandHandler(
        ICustomerUserRepository customerUserRepository,
        IUserRepository userRepository,
        IAuditLogWriter auditLogWriter)
    {
        _customerUserRepository = customerUserRepository;
        _userRepository = userRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<CustomerUserDetailResult> Handle(
        UpdateCustomerUserCommand request,
        CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new DomainException("CUSTOMER_ID_REQUIRED", DomainErrorType.Validation);
        }

        var existingCustomer = await _customerUserRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);

        var email = UserProfileRules.NormalizeEmail(request.Email);
        var fullName = UserProfileRules.NormalizeOptionalFullName(request.FullName);
        var phoneNumber = UserProfileRules.NormalizeOptionalPhoneNumber(request.PhoneNumber);

        if (await _userRepository.EmailExistsExceptCustomerAsync(email, request.CustomerId, ct))
        {
            throw new DomainException("EMAIL_ALREADY_EXISTS", DomainErrorType.Conflict);
        }

        if (phoneNumber is not null &&
            await _userRepository.PhoneNumberExistsExceptCustomerAsync(phoneNumber, request.CustomerId, ct))
        {
            throw new DomainException("PHONE_NUMBER_ALREADY_EXISTS", DomainErrorType.Conflict);
        }

        await _userRepository.UpdateCustomerProfileAsync(
            request.CustomerId,
            email,
            fullName,
            phoneNumber,
            ct);

        var updatedCustomer = existingCustomer with
        {
            Email = email,
            FullName = fullName ?? string.Empty,
            PhoneNumber = phoneNumber
        };

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.Customer,
            request.CustomerId,
            JsonSerializer.Serialize(new
            {
                existingCustomer.Email,
                existingCustomer.FullName,
                existingCustomer.PhoneNumber
            }),
            JsonSerializer.Serialize(new { email, fullName, phoneNumber }),
            null));

        return updatedCustomer;
    }

}
