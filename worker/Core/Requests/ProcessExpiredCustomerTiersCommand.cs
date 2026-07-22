using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record ProcessExpiredCustomerTiersCommand(DateTime ProcessedAt)
    : IRequest<int>, ITransactionalRequest;
