using Core.Abstractions;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.UseCases.Notifications.Handlers;

public sealed class GetEventTypesQueryHandler : IRequestHandler<GetEventTypesQuery, IReadOnlyCollection<NotificationEventTypeResult>>
{
    private readonly INotificationEventTypeRepository _repository;

    public GetEventTypesQueryHandler(INotificationEventTypeRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyCollection<NotificationEventTypeResult>> Handle(GetEventTypesQuery request, CancellationToken ct)
    {
        return _repository.GetListAsync(request.Keyword, ct);
    }
}
