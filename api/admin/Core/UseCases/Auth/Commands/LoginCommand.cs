using Core.UseCases.Auth.Results;
using MediatR;
using Core.Abstractions;

namespace Core.UseCases.Auth.Commands;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<LoginResult>, ITransactionalRequest;
