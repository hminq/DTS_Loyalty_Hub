using Core.UseCases.Permissions.Results;
using MediatR;

namespace Core.UseCases.Permissions.Queries;

public sealed record GetPermissionMatrixQuery : IRequest<IReadOnlyCollection<PermissionGroupResult>>;
