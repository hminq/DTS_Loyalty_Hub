using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth.Queries;

public sealed record GetCurrentAdminQuery(
    Guid UserId,
    Guid AdminId,
    Guid AdminSessionId,
    Guid AccessTokenJti) : IRequest<CurrentAdminResult>;
