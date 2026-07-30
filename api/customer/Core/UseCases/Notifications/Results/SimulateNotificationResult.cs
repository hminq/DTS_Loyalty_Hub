using System;

namespace Core.UseCases.Notifications.Results;

public sealed record SimulateNotificationResult(
    string Title,
    string Body,
    string Channel
);
