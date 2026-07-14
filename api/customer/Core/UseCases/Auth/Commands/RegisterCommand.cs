using Core.UseCases.Auth.Results;
using MediatR;

namespace Core.UseCases.Auth.Commands;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Phone) : IRequest<RegisterResult>;
