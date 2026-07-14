using Core.UseCases.AdminUsers.Results;
using MediatR;

namespace Core.UseCases.AdminUsers.Queries;

public sealed record GetAdminUserByIdQuery(Guid AdminId) : IRequest<AdminUserResult>;
