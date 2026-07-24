using Core.Abstractions;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Commands;

public sealed record CreateVoucherPoolImportJobCommand(
    Guid VoucherDefinitionId,
    string ImportFileKey,
    Guid? ActorUserId) : IRequest<VoucherPoolProvisioningResult>, ITransactionalRequest;
