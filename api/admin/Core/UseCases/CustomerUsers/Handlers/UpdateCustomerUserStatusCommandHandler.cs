using System.Text.Json;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.CustomerUsers.Commands;
using MediatR;

namespace Core.UseCases.CustomerUsers.Handlers;

public sealed class UpdateCustomerUserStatusCommandHandler
    : IRequestHandler<UpdateCustomerUserStatusCommand>
{
    private readonly ICustomerUserRepository _customerUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateCustomerUserStatusCommandHandler(
        ICustomerUserRepository customerUserRepository,
        IUserRepository userRepository,
        IAuditLogWriter auditLogWriter)
    {
        _customerUserRepository = customerUserRepository;
        _userRepository = userRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task Handle(UpdateCustomerUserStatusCommand request, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new DomainException("CUSTOMER_ID_REQUIRED", DomainErrorType.Validation);
        }

        var normalizedStatus = request.Status.Trim().ToUpperInvariant();
        if (normalizedStatus is not UserStatus.Enable and not UserStatus.Disable)
        {
            throw new DomainException("STATUS_INVALID", DomainErrorType.Validation);
        }

        var existingCustomer = await _customerUserRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);

        await _userRepository.UpdateCustomerStatusAsync(request.CustomerId, normalizedStatus, ct);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.UpdateStatus,
            AuditEntityTypes.Customer,
            request.CustomerId,
            JsonSerializer.Serialize(new { status = existingCustomer.Status }),
            JsonSerializer.Serialize(new { status = normalizedStatus }),
            null));
    }
}
