using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Core.UseCases.Notifications.Handlers;

public sealed class GetNotificationLogsQueryHandler : IRequestHandler<GetNotificationLogsQuery, PagedResult<NotificationLogResult>>
{
    private readonly INotificationLogRepository _repository;

    public GetNotificationLogsQueryHandler(INotificationLogRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<NotificationLogResult>> Handle(GetNotificationLogsQuery request, CancellationToken ct)
    {
        return _repository.GetPagedAsync(request.Page, request.PageSize, request.CustomerId, request.EventTypeCode, ct);
    }
}
