using Core.UseCases.Notifications.Results;
using MediatR;
using System.Collections.Generic;

using Core.UseCases.Common;

namespace Core.UseCases.Notifications.Queries;

public record GetNotificationTemplatesQuery(
    int Page,
    int PageSize,
    string? Keyword = null,
    string? EventTypeCode = null,
    string? Channel = null,
    string? Language = null,
    bool? IsActive = null) : IRequest<PagedResult<NotificationTemplateResult>>;
