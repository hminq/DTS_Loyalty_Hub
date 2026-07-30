using MediatR;
using Core.UseCases.Notifications.Results;

namespace Core.UseCases.Notifications.Queries;

public sealed record GetNotificationCodesQuery : IRequest<IReadOnlyCollection<NotificationCodeResult>>;
