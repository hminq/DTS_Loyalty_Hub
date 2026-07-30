using Core.Entities.Constants;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;

namespace Core.UseCases.Notifications.Handlers;

public sealed class GetNotificationCodesQueryHandler
    : IRequestHandler<GetNotificationCodesQuery, IReadOnlyCollection<NotificationCodeResult>>
{
    public Task<IReadOnlyCollection<NotificationCodeResult>> Handle(
        GetNotificationCodesQuery request,
        CancellationToken ct)
    {
        IReadOnlyCollection<NotificationCodeResult> result = NotificationCodes.All
            .Select(code => new NotificationCodeResult(
                code.Code,
                code.DisplayName,
                code.Description,
                code.InputFields
                    .Select(field => new NotificationInputFieldResult(
                        field.Key,
                        field.DisplayName,
                        field.DataType,
                        field.Description))
                    .ToArray(),
                NotificationSystemFields.All
                    .Select(field => new NotificationSystemFieldResult(
                        field.Key,
                        field.DisplayName,
                        field.DataType))
                    .ToArray()))
            .ToArray();

        return Task.FromResult(result);
    }
}
