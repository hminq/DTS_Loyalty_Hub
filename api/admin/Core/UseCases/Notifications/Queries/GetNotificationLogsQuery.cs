using Core.UseCases.Common;
using Core.UseCases.Notifications.Results;
using MediatR;
using System;

namespace Core.UseCases.Notifications.Queries;

public record GetNotificationLogsQuery(
    int Page, 
    int PageSize, 
    Guid? CustomerId = null, 
    string? EventTypeCode = null) : IRequest<PagedResult<NotificationLogResult>>;
