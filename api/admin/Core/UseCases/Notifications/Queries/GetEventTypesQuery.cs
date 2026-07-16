using Core.UseCases.Notifications.Results;
using MediatR;
using System.Collections.Generic;

namespace Core.UseCases.Notifications.Queries;

public record GetEventTypesQuery(string? Keyword = null) : IRequest<IReadOnlyCollection<NotificationEventTypeResult>>;
