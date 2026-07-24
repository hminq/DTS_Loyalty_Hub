using System;
using Core.UseCases.Notifications.Results;
using MediatR;

namespace Core.UseCases.Notifications.Commands.Simulate;

public sealed record SimulateNotificationCommand(
    Guid CustomerId,
    string EventTypeCode
) : IRequest<SimulateNotificationResult>;
