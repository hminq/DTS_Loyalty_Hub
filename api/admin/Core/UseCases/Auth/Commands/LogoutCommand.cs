using Core.Abstractions;
using MediatR;

namespace Core.UseCases.Auth.Commands;

public sealed record LogoutCommand(
    Guid AdminId,
    Guid AdminSessionId,
    Guid AccessTokenJti) : IRequest, IWriteRequest;
