using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record CompleteVoucherPoolImportCommand(
    Guid JobId,
    DateTime CompletedAt) : IRequest, ITransactionalRequest;
