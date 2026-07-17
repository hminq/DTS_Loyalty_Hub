using Core.Abstractions;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Core.UseCases.Common;

namespace Core.UseCases.Notifications.Handlers;

public sealed class GetNotificationTemplatesQueryHandler : IRequestHandler<GetNotificationTemplatesQuery, PagedResult<NotificationTemplateResult>>
{
    private readonly INotificationTemplateRepository _repository;

    public GetNotificationTemplatesQueryHandler(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<NotificationTemplateResult>> Handle(GetNotificationTemplatesQuery request, CancellationToken ct)
    {
        return _repository.GetPagedAsync(
            request.Page, request.PageSize,
            request.Keyword, request.EventTypeCode, request.Channel, request.Language, request.IsActive,
            ct);
    }
}
