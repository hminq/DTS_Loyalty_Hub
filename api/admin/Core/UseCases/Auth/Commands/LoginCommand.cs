using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth.Commands;

public sealed record LoginCommand(string Username, string Password) : IRequest<LoginResult>;
